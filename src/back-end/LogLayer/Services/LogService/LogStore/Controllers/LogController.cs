using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogStore.Business;
using LogStore.Models.Requests;
using LogStore.Models.Responses;

namespace LogStore.Controllers;

// ============================================================
// Log Service (Cross-Layer Logging)
// Handles: Audit, Error, and Failover log queries
// ------------------------------------------------------------
// Consumed by: Admin Dashboard, Analytics Console, Maintenance Tools
// Provides: Filtered queries across layers (Sensor, Traffic, User)
// ============================================================

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

    // ============================================================
    // GET: /api/logs/search
    // Roles: Admin
    // Purpose: Query logs with optional filters (Layer, Service, Type, Date Range)
    // ============================================================
    [HttpGet("search")]
    //[Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SearchLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    // ============================================================
    // POST: /api/logs/export
    // Roles: Admin
    // Purpose: Export filtered logs to CSV or PDF
    // ============================================================
    [HttpPost("export")]
    //[Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportLogs([FromBody] SearchLogRequest request, [FromQuery] string format = "csv")
    {
        _logger.LogInformation(
            "[CONTROLLER] Log export requested by {User} | Format={Format} | Layer={Layer} | Service={Service} | Type={Type}",
            User.FindFirstValue(ClaimTypes.Email) ?? "Unknown", format, request.Layer, request.Service, request.Type);

        var logs = await _logBusiness.SearchLogsAsync(request.Layer, request.Service, request.Type, request.From, request.To);
        if (logs == null || !logs.Any())
        {
            _logger.LogWarning("[CONTROLLER] No logs found for export.");
            return NotFound();
        }

        // Default to CSV
        var csvBytes = await _logBusiness.ExportLogsToCsvAsync(logs);
        return File(csvBytes, "text/csv", $"logs_export_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }
}
