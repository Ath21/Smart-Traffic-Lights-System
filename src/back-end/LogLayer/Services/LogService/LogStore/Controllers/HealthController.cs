using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LogStore.Controllers
{
    [ApiController]
    [Route("log-service")]
    public class HealthController : ControllerBase
    {
        // Liveness Probe
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", service = "Log Service" });
        }
    }
}
