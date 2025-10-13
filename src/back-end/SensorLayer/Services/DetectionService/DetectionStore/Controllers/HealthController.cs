using Microsoft.AspNetCore.Mvc;
using DetectionStore.Domain; 

namespace DetectionStore.Controllers
{
    [ApiController]
    [Route("detection-service")]
    public class HealthController : ControllerBase
    {
        private readonly IntersectionContext _intersection;

        public HealthController(IntersectionContext intersection)
        {
            _intersection = intersection;
        }

        // Liveness: Service is running (no external deps)
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "Detection Service",
                intersection = new
                {
                    id = _intersection.Id,
                    name = _intersection.Name
                }
            });
        }
    }
}
