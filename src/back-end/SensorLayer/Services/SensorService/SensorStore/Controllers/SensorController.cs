using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorStore.Business;
using SensorStore.Models.Requests;

namespace SensorStore.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorsController : ControllerBase
{
    private readonly ISensorCountService _business;

    public SensorsController(ISensorCountService business)
    {
        _business = business;
    }

    // ============================================================
    // GET: api/sensors/local
    // Role: Anonymous
    // Description: Get the latest snapshot for THIS intersection
    // ============================================================
    [HttpGet("local")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLocalSnapshot()
    {
        var response = await _business.GetSensorDataAsync();
        return Ok(response);
    }

    // ============================================================
    // POST: api/sensors/report
    // Role: TrafficOperator, Admin
    // Description: Report new sensor data for THIS intersection
    // ============================================================
    [HttpPost("report")]
    //[Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> Report([FromBody] SensorReportRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _business.ReportSensorDataAsync(request);

        var snapshot = await _business.GetSensorDataAsync();
        return Ok(snapshot);
    }
}