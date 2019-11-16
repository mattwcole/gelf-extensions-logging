using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gelf.Extensions.Logging.Samples.AspNetCore3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Generating weather forecast");

            var random = new Random();

            return Enumerable.Range(1, 5).Select(index =>
            {
                var weatherForecast = new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = random.Next(-20, 55),
                    Summary = Summaries[random.Next(Summaries.Length)]
                };

                _logger.LogDebug("Weather will be {weather_summary} on {weather_date}", weatherForecast.Summary, weatherForecast.Date);

                return weatherForecast;
            });
        }
    }
}
