using Microsoft.AspNetCore.Mvc;
using SensorStore.Domain;

namespace SensorStore.Controllers;

[ApiController]
[Route("api/sensors")]
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
            intersection = new { _intersection.Id, _intersection.Name }
        });
    }
}
