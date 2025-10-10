using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogStore.Business;
using LogStore.Models.Responses;
using LogStore.Models.Requests;

namespace LogStore.Controllers;

[ApiController]
[Route("api/logs")]
public class LogController : ControllerBase
{
    private readonly ILogBusiness _logBusiness;

    public LogController(ILogBusiness logBusiness)
    {
        _logBusiness = logBusiness;
    }

    // ============================================================
    // GET: /api/logs/search
    // Roles: Admin
    // Purpose: Query logs with optional filters
    // ============================================================
    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<SearchLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchLogs([FromQuery] SearchLogRequest request)
    {
        var logs = await _logBusiness.SearchLogsAsync(
            request.Layer, request.Service, request.Type, request.From, request.To);

        if (!logs.Any())
            return NotFound();

        return Ok(logs);
    }

}
