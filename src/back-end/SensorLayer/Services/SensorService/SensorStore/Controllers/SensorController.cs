using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorStore.Business;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Controllers;

// ============================================================
// Sensor Layer / Sensor Service - Raw Sensor Data
//
// Provides real-time and historical sensor snapshots.
// ============================================================

[ApiController]
[Route("api/sensors")]
public class SensorsController : ControllerBase
{
    private readonly ISensorCountService _business;
    private readonly IMapper _mapper;

    public SensorsController(ISensorCountService business, IMapper mapper)
    {
        _business = business;
        _mapper = mapper;
    }

    // ============================================================
    // GET: api/sensors/{intersectionId}
    // Role: Anonymous
    // Description: Get the latest sensor snapshot for a specific intersection.
    // ============================================================
    [HttpGet("{intersectionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSnapshot(Guid intersectionId)
    {
        var dto = await _business.GetSnapshotAsync(intersectionId);
        if (dto == null) return NotFound();

        return Ok(_mapper.Map<SensorSnapshotResponse>(dto));
    }

    // ============================================================
    // GET: api/sensors/{intersectionId}/history
    // Role: TrafficOperator, Admin
    // Description: Get historical sensor snapshots for a specific intersection.
    // ============================================================
    [HttpGet("{intersectionId:guid}/history")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> GetHistory(Guid intersectionId)
    {
        var dtos = await _business.GetHistoryAsync(intersectionId);
        return Ok(_mapper.Map<IEnumerable<SensorHistoryResponse>>(dtos));
    }

    // ============================================================
    // POST: api/sensors/update
    // Role: TrafficOperator, Admin
    // Description: Update the sensor snapshot for a specific intersection.
    // ============================================================
    [HttpPost("update")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateSensorSnapshotRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.SensorSnapshotDto>(request);
        var updated = await _business.UpdateSnapshotAsync(dto, request.AvgSpeed);

        return Ok(_mapper.Map<SensorSnapshotResponse>(updated));
    }
}
