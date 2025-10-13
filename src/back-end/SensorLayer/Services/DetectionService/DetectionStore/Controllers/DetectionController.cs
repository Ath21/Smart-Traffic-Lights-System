using Microsoft.AspNetCore.Mvc;
using DetectionStore.Business;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Controllers;

// ============================================================
// Detection Service (Sensor Layer)
// Handles: Emergency Vehicle, Public Transport, and Incident Detections
// ------------------------------------------------------------
// Consumed by: Intersection Controller Service, Traffic Analytics Service
// Publishes via RabbitMQ topics: sensor.detection.{intersection}.{event}
// ============================================================

[ApiController]
[Route("api/detections")]
public class DetectionController : ControllerBase
{
    private readonly IDetectionBusiness _service;
    private readonly ILogger<DetectionController> _logger;

    public DetectionController(IDetectionBusiness service, ILogger<DetectionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ============================================================
    // EMERGENCY VEHICLE DETECTIONS
    // ============================================================
    [HttpGet("emergency/{intersectionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<EmergencyVehicleDetectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecentEmergencies(int intersectionId)
    {
        var detections = await _service.GetRecentEmergenciesAsync(intersectionId);
        if (detections == null || !detections.Any())
        {
            _logger.LogWarning("[CONTROLLER] No emergency detections found for intersection {Id}", intersectionId);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} emergency detections returned for intersection {Id}",
            detections.Count(), intersectionId);
        return Ok(detections);
    }

    // ============================================================
    // PUBLIC TRANSPORT DETECTIONS
    // ============================================================
    [HttpGet("public-transport/{intersectionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<PublicTransportDetectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicTransports(int intersectionId)
    {
        var detections = await _service.GetPublicTransportsAsync(intersectionId);
        if (detections == null || !detections.Any())
        {
            _logger.LogWarning("[CONTROLLER] No public transport detections found for intersection {Id}", intersectionId);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} public transport detections returned for intersection {Id}",
            detections.Count(), intersectionId);
        return Ok(detections);
    }

    // ============================================================
    // INCIDENT DETECTIONS
    // ============================================================
    [HttpGet("incident/{intersectionId:int}")]
    [ProducesResponseType(typeof(IEnumerable<IncidentDetectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIncidents(int intersectionId)
    {
        var detections = await _service.GetRecentIncidentsAsync(intersectionId);
        if (detections == null || !detections.Any())
        {
            _logger.LogWarning("[CONTROLLER] No incident detections found for intersection {Id}", intersectionId);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} incident detections returned for intersection {Id}",
            detections.Count(), intersectionId);
        return Ok(detections);
    }
}
