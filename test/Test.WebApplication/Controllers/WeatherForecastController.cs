using Microsoft.AspNetCore.Mvc;

namespace Test.WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        [HttpGet(Name = "Get")]
        public IEnumerable<int> Get()
        {
            return Enumerable.Range(1, 5);
        }
    }
}