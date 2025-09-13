using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DetectionStore.Business;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Controllers;

// ============================================================
// Sensor Layer / Detection Service - Detection Events
//
// Handles detection of vehicles, incidents, and anomalies.
// ===========================================================

[ApiController]
[Route("api/detections")]
public class DetectionController : ControllerBase
{
    private readonly ISensorDetectionService _business;
    private readonly IMapper _mapper;

    public DetectionController(ISensorDetectionService business, IMapper mapper)
    {
        _business = business;
        _mapper = mapper;
    }

    // ============================================================
    // GET: api/detections/{intersectionId}
    // Role: Anonymous
    // Description: Get the latest detection snapshot for a specific intersection.
    // ============================================================
    [HttpGet("{intersectionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSnapshot(Guid intersectionId)
    {
        var dto = await _business.GetSnapshotAsync(intersectionId);
        if (dto == null) return NotFound();

        return Ok(_mapper.Map<DetectionSnapshotResponse>(dto));
    }

    // ============================================================
    // GET: api/detections/{intersectionId}/history
    // Role: TrafficOperator, Admin
    // Description: Get historical detection data for a specific intersection.
    // ============================================================
    [HttpGet("{intersectionId:guid}/history")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> GetHistory(Guid intersectionId)
    {
        var dtos = await _business.GetHistoryAsync(intersectionId);
        return Ok(_mapper.Map<IEnumerable<DetectionHistoryResponse>>(dtos));
    }

    // ============================================================
    // POST: api/detections/emergency
    // Role: TrafficOperator, Admin
    // Description: Record an emergency vehicle detection event.
    // ============================================================
    [HttpPost("emergency")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> RecordEmergency([FromBody] RecordEmergencyVehicleRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.EmergencyVehicleDto>(request);
        var result = await _business.RecordEmergencyAsync(dto);

        return Ok(_mapper.Map<EmergencyVehicleResponse>(result));
    }

    // ============================================================
    // POST: api/detections/public-transport
    // Role: TrafficOperator, Admin
    // Description: Record a public transport detection event.
    // ============================================================
    [HttpPost("public-transport")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> RecordPublicTransport([FromBody] RecordPublicTransportRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.PublicTransportDto>(request);
        var result = await _business.RecordPublicTransportAsync(dto);

        return Ok(_mapper.Map<PublicTransportResponse>(result));
    }

    // ============================================================
    // POST: api/detections/incident
    // Role: TrafficOperator, Admin
    // Description: Record an incident detection event.
    // ============================================================
    [HttpPost("incident")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> RecordIncident([FromBody] RecordIncidentRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.IncidentDto>(request);
        var result = await _business.RecordIncidentAsync(dto);

        return Ok(_mapper.Map<IncidentResponse>(result));
    }
}
