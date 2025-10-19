using Microsoft.AspNetCore.Mvc;
using SensorStore.Business;
using SensorStore.Domain;
using SensorStore.Models.Responses;

namespace SensorStore.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorController : ControllerBase
{
    private readonly ISensorBusiness _business;
    private readonly ILogger<SensorController> _logger;
    private readonly IntersectionContext _intersection;

    public SensorController(
        ISensorBusiness business,
        ILogger<SensorController> logger,
        IntersectionContext intersection)
    {
        _business = business;
        _logger = logger;
        _intersection = intersection;
    }

    [HttpGet]
    [Route("vehicle")]
    public async Task<IActionResult> GetVehicleCounts()
    {
        var data = await _business.GetRecentVehicleCountsAsync(_intersection.Id);
        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER][SENSOR] No vehicle counts found for intersection {Name}", _intersection.Name);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER][SENSOR] {Count} vehicle records returned for {Intersection}",
            data.Count(), _intersection.Name);

        return Ok(data);
    }

    [HttpGet]
    [Route("pedestrian")]
    public async Task<IActionResult> GetPedestrianCounts()
    {
        var data = await _business.GetRecentPedestrianCountsAsync(_intersection.Id);
        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER][SENSOR] No pedestrian counts found for intersection {Name}", _intersection.Name);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER][SENSOR] {Count} pedestrian records returned for {Intersection}",
            data.Count(), _intersection.Name);

        return Ok(data);
    }

    [HttpGet]
    [Route("cyclist")]
    public async Task<IActionResult> GetCyclistCounts()
    {
        var data = await _business.GetRecentCyclistCountsAsync(_intersection.Id);
        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER][SENSOR] No cyclist counts found for intersection {Name}", _intersection.Name);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER][SENSOR] {Count} cyclist records returned for {Intersection}",
            data.Count(), _intersection.Name);

        return Ok(data);
    }
}
