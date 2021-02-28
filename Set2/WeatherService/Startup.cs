using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Registry;
using WeatherService.Services;

namespace WeatherService
{
    public class Startup
    {
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherService", Version = "v1" });
            });
            
            var policyRegistry = AddInMemoryCache(services);
            //var policyRegistry = AddDistributedCache(services);

            services.AddHttpClient<ITemperatureService, TemperatureService>("TemperatureService", client =>
                {
                    client.BaseAddress = new Uri(Configuration["temperatureServiceUrl"]);
                }).AddPolicyHandlerFromRegistry((pairs, message) => 
                    policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("CachingPolicy"));
        }
        

        private static IPolicyRegistry<string> AddInMemoryCache(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
            var policyRegistry = services.AddPolicyRegistry();
            return policyRegistry;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAsyncCacheProvider cacheProvider, IPolicyRegistry<string> registry)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherService v1"));


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
            
            registry.Add("CachingPolicy", Policy.CacheAsync<HttpResponseMessage>(cacheProvider, TimeSpan.FromSeconds(30)));
        }
    }
}
