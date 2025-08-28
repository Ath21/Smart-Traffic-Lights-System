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

    // GET: /api/traffic/analytics/congestion/{intersectionId}
    [HttpGet("congestion/{intersectionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCongestion(Guid intersectionId)
    {
        var dto = await _service.GetCurrentCongestionAsync(intersectionId);
        if (dto == null) return NotFound(new { error = "No congestion data available" });

        var response = _mapper.Map<CongestionResponse>(dto);
        return Ok(response);
    }

    // GET: /api/traffic/analytics/incidents/{intersectionId}
    [HttpGet("incidents/{intersectionId}")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<IActionResult> GetIncidents(Guid intersectionId)
    {
        var dtos = await _service.GetIncidentsAsync(intersectionId);
        var responses = _mapper.Map<IEnumerable<IncidentResponse>>(dtos);
        return Ok(responses);
    }

    // GET: /api/traffic/analytics/summary/{intersectionId}/{date}
    [HttpGet("summary/{intersectionId}/{date}")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<IActionResult> GetSummary(Guid intersectionId, DateTime date)
    {
        var dto = await _service.GetDailySummaryAsync(intersectionId, date);
        if (dto == null) return NotFound(new { error = "No summary available for given date" });

        var response = _mapper.Map<SummaryResponse>(dto);
        return Ok(response);
    }

    // GET: /api/traffic/analytics/reports/daily
    [HttpGet("reports/daily")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<IActionResult> GetDailyReport()
    {
        var dtos = await _service.GetDailyReportsAsync();
        var responses = _mapper.Map<IEnumerable<SummaryResponse>>(dtos);
        return Ok(responses);
    }
}
