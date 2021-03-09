using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace WeatherService.Services
{
    public interface ITemperatureService
    {
        Task<HttpResponseMessage> GetFahrenheit(int locationId);
        Task<HttpResponseMessage> GetCelsius(int locationId);
    }

    public class TemperatureService : ITemperatureService
    {
        private readonly HttpClient _client;

        public TemperatureService(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.Add("Accept", "application/json");

        }
        public async Task<HttpResponseMessage> GetFahrenheit(int locationId)
        {
            return await _client.GetAsync($"fahrenheit/{locationId}");
        }

        public async Task<HttpResponseMessage> GetCelsius(int locationId)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                new Uri(_client.BaseAddress + $"celsius/{locationId}"));

            httpRequestMessage.SetPolicyExecutionContext(new Context($"GetCelsius-{locationId}"));

           return await _client.SendAsync(httpRequestMessage);
        }

    }
}
