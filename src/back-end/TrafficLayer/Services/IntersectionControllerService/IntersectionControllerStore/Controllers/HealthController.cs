using IntersectionControllerStore.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntersectionControllerStore.Controllers
{
    [ApiController]
    [Route("intersection_controller_service")]
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
                service = "Intersection Controller Service",
                intersection = new
                {
                    id = _intersection.Id,
                    name = _intersection.Name
                }
            });
        }
    }
}