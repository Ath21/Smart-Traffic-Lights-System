using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using TrafficAnalyticsStore.Business.Alerts;
using TrafficAnalyticsStore.Business.DailySummary;
using TrafficAnalyticsStore.Models;

namespace TrafficAnalyticsStore.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IDailySummaryBusiness _summaryService;
    private readonly IAlertBusiness _alertService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IDailySummaryBusiness summaryService,
        IAlertBusiness alertService,
        ILogger<AnalyticsController> logger)
    {
        _summaryService = summaryService;
        _alertService = alertService;
        _logger = logger;
    }

    // ============================================================
    // 1️⃣  Summaries for UI Graphs (JSON)
    // ============================================================
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<DailySummaryDto>>> GetSummaries(
        [FromQuery] int? intersectionId = null,
        [FromQuery] string? intersection = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var summaries = await _summaryService.GetSummariesAsync(intersectionId, intersection, from, to);
        return Ok(summaries);
    }

    // ============================================================
    // 2️⃣  Export Summaries as CSV
    // ============================================================
    [HttpGet("summaries/export")]
    public async Task<IActionResult> ExportSummariesCsv(
        [FromQuery] int? intersectionId = null,
        [FromQuery] string? intersection = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var bytes = await _summaryService.ExportSummariesCsvAsync(intersectionId, intersection, from, to);

        if (bytes.Length == 0)
            return NotFound("No data found for the given filters.");

        var fileName = $"traffic_summaries_{DateTime.UtcNow:yyyyMMddHHmm}.csv";
        return File(bytes, "text/csv", fileName);
    }

    // ============================================================
    // 3️⃣  Alerts (for notifications / admin)
    // ============================================================
    [HttpGet("alerts")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts(
        [FromQuery] string? type = null,
        [FromQuery] string? intersection = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var alerts = await _alertService.GetAlertsAsync(type, intersection, from, to);
        return Ok(alerts);
    }
}
