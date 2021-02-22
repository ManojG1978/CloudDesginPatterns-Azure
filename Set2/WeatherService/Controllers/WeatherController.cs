using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherService.Services;

namespace WeatherService.Controllers
{
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly ITemperatureService _temperatureService;

        private async Task<ActionResult> ProcessResponse(HttpResponseMessage httpResponseMessage)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return Ok(content);
            }

            return StatusCode((int) httpResponseMessage.StatusCode, content);
        }

        public WeatherController(ITemperatureService temperatureService)
        {
            _temperatureService = temperatureService;
        }

        [HttpGet("fahrenheit/{locationId}")]
        public async Task<ActionResult> GetFahrenheit(int locationId)
        {
            return await ProcessResponse(await _temperatureService.GetFahrenheit(locationId));
        }

        [HttpGet("celsius/{locationId}")]
        public async Task<ActionResult> GetCelsius(int locationId)
        {
            return await ProcessResponse(await _temperatureService.GetCelsius(locationId));
        }

    }
}
