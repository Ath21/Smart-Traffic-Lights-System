using Microsoft.AspNetCore.Mvc;
using SensorStore.Business;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Controllers;

// ============================================================
// Sensor Service (Fog Layer)
// ------------------------------------------------------------
// Handles: Vehicle, Pedestrian, and Cyclist count measurements
// Persists to MongoDB and synchronizes to Redis cache
// ------------------------------------------------------------
// Consumed by: Detection Service, Traffic Analytics Service
// ============================================================

[ApiController]
[Route("api/sensors")]
public class SensorController : ControllerBase
{
    private readonly ISensorBusiness _business;
    private readonly ILogger<SensorController> _logger;

    public SensorController(ISensorBusiness business, ILogger<SensorController> logger)
    {
        _business = business;
        _logger = logger;
    }

    // ============================================================
    // VEHICLE COUNTS
    // ============================================================
    [HttpGet("vehicle/{intersectionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<VehicleCountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleCounts(int intersectionId)
    {
        var data = await _business.GetRecentVehicleCountsAsync(intersectionId);

        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER] No vehicle counts found for intersection {Id}", intersectionId);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} vehicle count records returned for intersection {Id}",
            data.Count(), intersectionId);

        return Ok(data);
    }

    // ============================================================
    // PEDESTRIAN COUNTS
    // ============================================================
    [HttpGet("pedestrian/{intersectionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<PedestrianCountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPedestrianCounts(int intersectionId)
    {
        var data = await _business.GetRecentPedestrianCountsAsync(intersectionId);

        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER] No pedestrian counts found for intersection {Id}", intersectionId);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} pedestrian count records returned for intersection {Id}",
            data.Count(), intersectionId);

        return Ok(data);
    }

    // ============================================================
    // CYCLIST COUNTS
    // ============================================================
    [HttpGet("cyclist/{intersectionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<CyclistCountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCyclistCounts(int intersectionId)
    {
        var data = await _business.GetRecentCyclistCountsAsync(intersectionId);

        if (!data.Any())
        {
            _logger.LogWarning("[CONTROLLER] No cyclist counts found for intersection {Id}", intersectionId);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} cyclist count records returned for intersection {Id}",
            data.Count(), intersectionId);

        return Ok(data);
    }
}
