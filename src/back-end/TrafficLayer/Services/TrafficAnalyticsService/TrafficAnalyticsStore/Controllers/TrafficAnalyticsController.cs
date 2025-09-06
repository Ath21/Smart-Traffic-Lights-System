using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using TrafficAnalyticsStore.Models.Responses;
using TrafficAnalyticsStore.Business;

namespace TrafficAnalyticsStore.Controllers;

[ApiController]
[Route("api/traffic/analytics")]
public class TrafficAnalyticsController : ControllerBase
{
    private readonly ITrafficAnalyticsService _service;
    private readonly IMapper _mapper;

    public TrafficAnalyticsController(ITrafficAnalyticsService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // ============================================================
    // GET: /api/traffic/analytics/congestion/{intersectionId}
    // Roles: Anonymous
    // Purpose: Get current congestion data for a specific intersection
    // ============================================================
    [HttpGet("congestion/{intersectionId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CongestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCongestion(Guid intersectionId)
    {
        var dto = await _service.GetCurrentCongestionAsync(intersectionId);
        if (dto == null) return NotFound(new { error = "No congestion data available" });

        var response = _mapper.Map<CongestionResponse>(dto);
        return Ok(response);
    }

    // ============================================================
    // GET: /api/traffic/analytics/incidents/{intersectionId}
    // Roles: User, TrafficOperator, Admin
    // Purpose: Get all incidents detected for a specific intersection
    // ============================================================
    [HttpGet("incidents/{intersectionId:guid}")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<IncidentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncidents(Guid intersectionId)
    {
        var dtos = await _service.GetIncidentsAsync(intersectionId);
        var responses = _mapper.Map<IEnumerable<IncidentResponse>>(dtos);
        return Ok(responses);
    }

    // ============================================================
    // GET: /api/traffic/analytics/summary/{intersectionId}/{date}
    // Roles: User, TrafficOperator, Admin
    // Purpose: Get daily traffic summary for a specific intersection and date
    // ============================================================
    [HttpGet("summary/{intersectionId:guid}/{date}")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(SummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(Guid intersectionId, DateTime date)
    {
        var dto = await _service.GetDailySummaryAsync(intersectionId, date);
        if (dto == null) return NotFound(new { error = "No summary available for given date" });

        var response = _mapper.Map<SummaryResponse>(dto);
        return Ok(response);
    }

    // ============================================================
    // GET: /api/traffic/analytics/reports/daily
    // Roles: User, TrafficOperator, Admin
    // Purpose: Get all daily traffic reports (system-wide)
    // ============================================================
    [HttpGet("reports/daily")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<SummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyReport()
    {
        var dtos = await _service.GetDailyReportsAsync();
        var responses = _mapper.Map<IEnumerable<SummaryResponse>>(dtos);
        return Ok(responses);
    }
}
