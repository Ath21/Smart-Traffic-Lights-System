using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogStore.Business;
using LogStore.Models.Responses;
using LogStore.Models.Requests;

namespace LogStore.Controllers;

// ============================================================
// Log Layer / Log Service - Audit & Error Logs
//
// Centralized logging and auditing.

// ===========================================================

[ApiController]
[Route("api/logs")]
public class LogController : ControllerBase
{
    private readonly ILogService _logService;

    public LogController(ILogService logService)
    {
        _logService = logService;
    }

    // ============================================================
    // GET: /api/logs/audit/{serviceName}
    // Roles: Admin
    // Purpose: Query audit logs by service
    // ============================================================
    [HttpGet("audit/{serviceName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<AuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AuditLogResponse>>> GetAuditLogs(string serviceName)
    {
        var logs = await _logService.GetAuditLogsByServiceAsync(serviceName);
        if (logs == null || logs.Count == 0)
            return NotFound($"No audit logs found for service: {serviceName}");

        return Ok(logs);
    }

    // ============================================================
    // GET: /api/logs/error/{serviceName}
    // Roles: Admin
    // Purpose: Query error logs by service
    // ============================================================
    [HttpGet("error/{serviceName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<ErrorLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ErrorLogResponse>>> GetErrorLogs(string serviceName)
    {
        var logs = await _logService.GetErrorLogsByServiceAsync(serviceName);
        if (logs == null || logs.Count == 0)
            return NotFound($"No error logs found for service: {serviceName}");

        return Ok(logs);
    }

    // ============================================================
    // GET: /api/logs/search
    // Roles: Admin
    // Purpose: Filter logs by metadata / timeframe
    // ============================================================
    [HttpGet("search")]
    //[Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<object>>> SearchLogs([FromQuery] SearchLogsRequest request)
    {
        var logs = await _logService.SearchLogsAsync(
            request.ServiceName,
            request.ErrorType,
            request.Action,
            request.From,
            request.To,
            request.Metadata);

        return Ok(logs);
    }
}
