using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficLightData;

namespace TrafficLightCoordinatorStore.Controllers;

[ApiController]
[Route("api/trafficlights")]
public class TrafficLightsController : ControllerBase
{
    private readonly TrafficLightDbContext _db;
    private readonly ILogger<TrafficLightsController> _logger;

    public TrafficLightsController(TrafficLightDbContext db, ILogger<TrafficLightsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /api/trafficlights
    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll()
    {
        var lights = await _db.TrafficLights
            .AsNoTracking()
            .Include(l => l.Intersection)
            .OrderBy(l => l.LightId)
            .Select(l => new
            {
                l.LightId,
                l.LightName,
                l.Direction,
                Coordinates = new { l.Latitude, l.Longitude },
                l.IsOperational,
                Intersection = new
                {
                    l.IntersectionId,
                    l.Intersection!.Name
                }
            })
            .ToListAsync();

        return Ok(lights);
    }

    // GET: /api/trafficlights/{id}
    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var light = await _db.TrafficLights
            .AsNoTracking()
            .Include(l => l.Intersection)
            .FirstOrDefaultAsync(l => l.LightId == id);

        if (light is null)
            return NotFound(new { error = $"Traffic light ID {id} not found" });

        var response = new
        {
            light.LightId,
            light.LightName,
            light.Direction,
            Coordinates = new { light.Latitude, light.Longitude },
            light.IsOperational,
            Intersection = new
            {
                light.IntersectionId,
                light.Intersection?.Name
            }
        };

        return Ok(response);
    }
}
