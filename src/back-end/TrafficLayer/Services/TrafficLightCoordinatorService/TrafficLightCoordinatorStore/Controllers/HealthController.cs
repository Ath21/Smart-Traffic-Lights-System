using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TrafficLightCoordinatorStore.Controllers
{
    [ApiController]
    [Route("traffic_light_coordinator_service")]
    public class HealthController : ControllerBase
    {
        // Liveness: Service is running (no external deps)
        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", service = "Traffic Light Coordinator Service" });
        }
    }
}
