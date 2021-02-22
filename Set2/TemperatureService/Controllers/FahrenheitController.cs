using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace TemperatureService.Controllers
{
    [Route("[controller]")]
    public class FahrenheitController : ControllerBase
    {
        [HttpGet("{locationId}")]
        public ActionResult Get(int locationId)
        {
            // fails 100% of the time
            return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong when getting the temperature in fahrenheit.");
        }
    }
}
