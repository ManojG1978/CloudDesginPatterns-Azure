using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using WeatherService.Services;

namespace WeatherService
{
    public class Startup
    {
        private ILogger _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "WeatherService", Version = "v1"});
            });

            var advancedCircuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .AdvancedCircuitBreakerAsync(0.25, TimeSpan.FromSeconds(60), 7, TimeSpan.FromSeconds(30),
                    OnBreak, OnReset, OnHalfOpen);

            var basicCircuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30), OnBreak, OnReset, OnHalfOpen);

            services.AddHttpClient<ITemperatureService, TemperatureService>("TemperatureService")
                .AddPolicyHandler(basicCircuitBreakerPolicy)
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromMilliseconds(100),
                        TimeSpan.FromMilliseconds(500),
                    }));

            services.AddHealthChecks()
                .AddCheck("Temperature Service", () =>
                {
                    return basicCircuitBreakerPolicy.CircuitState switch
                    {
                        CircuitState.Open => HealthCheckResult.Unhealthy("Circuit Breaker is in Open State"),
                        CircuitState.HalfOpen => HealthCheckResult.Degraded("Circuit Breaker is in Half Open State"),
                        _ => HealthCheckResult.Healthy()
                    };
                });

            services.AddHealthChecksUI((settings => { settings.AddHealthCheckEndpoint("Weather Service", "/hc"); }))
                .AddInMemoryStorage();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<Program>();
        }

        private void OnHalfOpen()
        {
            _logger.LogInformation("Circuit in test mode, one request will be allowed.");
        }

        private void OnReset()
        {
            _logger.LogInformation("Circuit closed, requests flow normally.");
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> result, TimeSpan ts)
        {
            _logger.LogInformation("Circuit cut, requests will not flow.");
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherService v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });

                endpoints.MapHealthChecksUI(options => { options.UIPath = "/hc-ui"; });
            });
        }
    }
}