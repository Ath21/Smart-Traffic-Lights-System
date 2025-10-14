using Microsoft.AspNetCore.Mvc;
using TrafficLightControllerStore.Domain;

namespace TrafficLightControllerStore.Controllers;

[ApiController]
[Route("traffic_light_controller_service")]
public class HealthController : ControllerBase
{
    private readonly IntersectionContext _intersection;
    private readonly TrafficLightContext _light;

    public HealthController(IntersectionContext intersection, TrafficLightContext light)
    {
        _intersection = intersection;
        _light = light;
    }

    // Liveness: Service is running (no external deps)
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "Traffic Light Controller Service",
            intersection = new
            {
                id = _intersection.Id,
                name = _intersection.Name
            },
            light = new
            {
                id = _light.Id,
                name = _light.Name
            }
        });
    }
}
