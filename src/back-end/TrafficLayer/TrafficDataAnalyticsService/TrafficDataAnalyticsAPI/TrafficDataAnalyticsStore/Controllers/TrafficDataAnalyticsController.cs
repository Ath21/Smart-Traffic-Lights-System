using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using TrafficDataAnalyticsStore.Models.Responses;
using TrafficDataAnalyticsStore.Business;

namespace TrafficDataAnalyticsStore.Controllers;

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

    [HttpGet("congestion/{intersectionId}")]
    public async Task<IActionResult> GetCongestion(Guid intersectionId)
    {
        var dto = await _service.GetCurrentCongestionAsync(intersectionId);
        if (dto == null) return NotFound(new { error = "No congestion data available" });

        var response = _mapper.Map<CongestionResponse>(dto);
        return Ok(response);
    }

    [HttpGet("incidents/{intersectionId}")]
    public async Task<IActionResult> GetIncidents(Guid intersectionId)
    {
        var dtos = await _service.GetIncidentsAsync(intersectionId);
        var responses = _mapper.Map<IEnumerable<IncidentResponse>>(dtos);
        return Ok(responses);
    }

    [HttpGet("summary/{intersectionId}/{date}")]
    public async Task<IActionResult> GetSummary(Guid intersectionId, DateTime date)
    {
        var dto = await _service.GetDailySummaryAsync(intersectionId, date);
        if (dto == null) return NotFound(new { error = "No summary available for given date" });

        var response = _mapper.Map<SummaryResponse>(dto);
        return Ok(response);
    }

    [HttpGet("reports/daily")]
    public async Task<IActionResult> GetDailyReport()
    {
        var dtos = await _service.GetDailyReportsAsync();
        var responses = _mapper.Map<IEnumerable<SummaryResponse>>(dtos);
        return Ok(responses);
    }
}
