using Microsoft.AspNetCore.Mvc;

namespace JetFlight.WebApi.Controllers
{

    [ApiController]
    [Route("v1/[controller]")]

    public class PublicController : BaseController
    {

        public PublicController()
        {
        }

        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            var version = Environment.GetEnvironmentVariable("VERSION");
            if (version == null)
            {
                version = "local";
            }
            return Ok(version);
        }
    }
}