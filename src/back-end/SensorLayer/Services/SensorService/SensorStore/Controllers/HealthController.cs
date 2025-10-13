using Microsoft.AspNetCore.Mvc;
using SensorStore.Domain;

namespace SensorStore.Controllers
{
    [ApiController]
    [Route("sensor-service")]
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
                service = "Sensor Service",
                intersection = new
                {
                    id = _intersection.Id,
                    name = _intersection.Name
                }
            });
        }
    }
}
