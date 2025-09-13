using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TrafficAnalyticsStore.Controllers
{
    [ApiController]
    [Route("traffic_analytics_service")]
    public class HealthController : ControllerBase
    {
        // Liveness: Service is running (no external deps)
        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", service = "Traffic Analytics Service" });
        }
    }
}