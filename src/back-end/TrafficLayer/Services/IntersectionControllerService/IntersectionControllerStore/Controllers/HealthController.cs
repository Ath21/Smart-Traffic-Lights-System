using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntersectionControllerStore.Controllers
{
    [ApiController]
    [Route("intersection_controller_service")]
    public class HealthController : ControllerBase
    {
        // Liveness: Service is running (no external deps)
        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", service = "Intersection Controller Service" });
        }
    }
}