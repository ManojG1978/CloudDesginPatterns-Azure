using System.Reflection;
using System.Threading.Tasks;
using CQRSAndMediator.Infrastructure;
using CQRSAndMediator.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CQRSAndMediator
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
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "OrderService", Version = "v1"});
            });

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddSingleton<ICosmosDbService>(
                InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(
            IConfigurationSection configurationSection)
        {
            var databaseName = configurationSection.GetSection("DatabaseName").Value;
            var readContainerName = configurationSection.GetSection("ReadContainerName").Value;
            var writeContainerName = configurationSection.GetSection("WriteContainerName").Value;
            var account = configurationSection.GetSection("Account").Value;
            var key = configurationSection.GetSection("Key").Value;
            var client = new CosmosClient(account, key);
            var cosmosDbService = new CosmosDbService(client, databaseName, readContainerName, writeContainerName);
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(readContainerName, "/orderId");

            //await database.Database.CreateContainerIfNotExistsAsync(writeContainerName, "/orderId");

            return cosmosDbService;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}