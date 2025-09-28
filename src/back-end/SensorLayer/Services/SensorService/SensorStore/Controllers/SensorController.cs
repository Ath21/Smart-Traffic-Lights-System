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
    private readonly SensorCountService _business;
    private readonly IMapper _mapper;

    public SensorsController(SensorCountService business, IMapper mapper)
    {
        _business = business;
        _mapper = mapper;
    }

    // ============================================================
    // GET: api/sensors/{intersectionId}
    // Role: Anonymous
    // Description: Get the latest sensor snapshot for a specific intersection.
    // ============================================================
    [HttpGet("{intersectionId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSnapshot(int intersectionId)
    {
        var response = await _business.GetSensorDataAsync(intersectionId);
        return Ok(response);
    }

    // ============================================================
    // POST: api/sensors/report
    // Role: TrafficOperator, Admin
    // Description: Report new sensor data for a specific intersection.
    // ============================================================
    [HttpPost("report")]
    //[Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> Report([FromBody] SensorReportRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _business.ReportSensorDataAsync(request);

        // We can return the current snapshot after reporting
        var snapshot = await _business.GetSensorDataAsync(request.IntersectionId);
        return Ok(snapshot);
    }
}
