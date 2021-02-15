using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace TemperatureService.Controllers
{
    [Route("[controller]")]
    public class CelsiusController : ControllerBase
    {
        private static int _counter = 0;
        private static readonly Random RandomTemperature = new Random();

        [HttpGet("{locationId}")]
        public ActionResult Get(int locationId)
        {
            _counter++;
            if (_counter % 4 != 0)
            {
                return Ok(RandomTemperature.Next(0, 100));
            }
            return StatusCode((int) HttpStatusCode.InternalServerError, "Something went wrong when getting the temperature.");
        }
    }
}
