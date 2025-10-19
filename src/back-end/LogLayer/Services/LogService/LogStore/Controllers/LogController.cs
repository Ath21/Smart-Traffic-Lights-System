using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogStore.Business;
using LogStore.Models.Requests;
using LogStore.Models.Responses;

namespace LogStore.Controllers;

[ApiController]
[Route("api/logs")]
public class LogController : ControllerBase
{
    private readonly ILogBusiness _logBusiness;
    private readonly ILogger<LogController> _logger;

    public LogController(ILogBusiness logBusiness, ILogger<LogController> logger)
    {
        _logBusiness = logBusiness;
        _logger = logger;
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> SearchLogs([FromQuery] SearchLogRequest request)
    {
        _logger.LogInformation(
            "[CONTROLLER] Log search requested by {User} | Layer={Layer} | Service={Service} | Type={Type} | From={From} | To={To}",
            User.FindFirstValue(ClaimTypes.Email) ?? "Unknown",
            request.Layer ?? "Any",
            request.Service ?? "Any",
            request.Type ?? "Any",
            request.From?.ToString("u") ?? "null",
            request.To?.ToString("u") ?? "null"
        );

        var logs = await _logBusiness.SearchLogsAsync(
            request.Layer, request.Service, request.Type, request.From, request.To);

        if (logs == null || !logs.Any())
        {
            _logger.LogWarning(
                "[CONTROLLER] No logs found for filters: Layer={Layer}, Service={Service}, Type={Type}, Range={From}–{To}",
                request.Layer, request.Service, request.Type, request.From, request.To);
            return NotFound();
        }

        _logger.LogInformation("[CONTROLLER] {Count} log entries returned for query.", logs.Count());
        return Ok(logs);
    }

    [HttpPost]
    [Route("export")]
    public async Task<IActionResult> ExportLogs([FromBody] SearchLogRequest request, [FromQuery] string format = "csv")
    {
        _logger.LogInformation(
            "[CONTROLLER] Log export requested by {User} | Format={Format} | Layer={Layer} | Service={Service} | Type={Type}",
            User.FindFirstValue(ClaimTypes.Email) ?? "Unknown",
            format, request.Layer, request.Service, request.Type);

        var logs = await _logBusiness.SearchLogsAsync(request.Layer, request.Service, request.Type, request.From, request.To);

        if (logs == null || !logs.Any())
        {
            _logger.LogWarning("[CONTROLLER] No logs found for export.");
            return NotFound();
        }

        var csvBytes = await _logBusiness.ExportLogsToCsvAsync(logs);
        return File(csvBytes, "text/csv", $"logs_export_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }
}
