using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WeatherService.Services
{
    public interface ITemperatureService
    {
        Task<HttpResponseMessage> GetFahrenheit(int locationId);
        Task<HttpResponseMessage> GetCelsius(int locationId);
    }

    public class TemperatureService : ITemperatureService
    {
        private IConfiguration Configuration { get; }
        private readonly HttpClient _client;

        public TemperatureService(HttpClient client, IConfiguration configuration)
        {
            Configuration = configuration;
            _client = client;
            _client.BaseAddress = new Uri(Configuration["TempServiceUrl"]); 
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            
        }
        public async Task<HttpResponseMessage> GetFahrenheit(int locationId)
        {
            return await _client.GetAsync($"fahrenheit/{locationId}");
        }

        public async Task<HttpResponseMessage> GetCelsius(int locationId)
        {
            return await _client.GetAsync($"celsius/{locationId}");
        }

    }
}
