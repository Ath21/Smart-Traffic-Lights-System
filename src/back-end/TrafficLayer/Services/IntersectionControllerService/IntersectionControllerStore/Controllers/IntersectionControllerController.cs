using AutoMapper;
using IntersectionControllerStore.Business.CommandLog;
using IntersectionControllerStore.Business.TrafficLight;
using IntersectionControllerStore.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntersectionControllerStore.Controllers;

// ============================================================
// Traffic Layer /Intersection Controller Service - Intersection State
//
// Exposes status and command history for intersections.
// ============================================================

[ApiController]
[Route("api/intersections")]
public class IntersectionController : ControllerBase
{
    private readonly ITrafficLightService _lightService;
    private readonly ICommandLogService _logService;
    private readonly IMapper _mapper;
    private readonly ILogger<IntersectionController> _logger;

    private const string ServiceTag = "[" + nameof(IntersectionController) + "]";

    public IntersectionController(
        ITrafficLightService lightService,
        ICommandLogService logService,
        IMapper mapper,
        ILogger<IntersectionController> logger)
    {
        _lightService = lightService;
        _logService = logService;
        _mapper = mapper;
        _logger = logger;
    }

    // ============================================================
    // GET: /api/intersections/status/{intersectionId}
    // Roles: User, TrafficOperator, Admin
    // Purpose: Retrieve live light states for a given intersection
    // ============================================================
    [HttpGet("status/{intersectionId:guid}")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<TrafficLightStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TrafficLightStatusResponse>>> GetStatus(Guid intersectionId)
    {
        _logger.LogInformation("{Tag} GET /status/{IntersectionId}", ServiceTag, intersectionId);

        var states = await _lightService.GetByIntersectionAsync(intersectionId);
        if (!states.Any())
            return NotFound($"No traffic light states found for intersection {intersectionId}");

        var response = _mapper.Map<IEnumerable<TrafficLightStatusResponse>>(states);
        return Ok(response);
    }

    // ============================================================
    // GET: /api/intersections/events/{intersectionId}
    // Roles: TrafficOperator, Admin
    // Purpose: Retrieve last applied control commands for a given intersection
    // ============================================================
    [HttpGet("events/{intersectionId:guid}")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<object>>> GetEvents(Guid intersectionId)
    {
        _logger.LogInformation("{Tag} GET /events/{IntersectionId}", ServiceTag, intersectionId);

        var events = await _logService.GetRecentCommandsAsync<object>(intersectionId);
        if (events == null || !events.Any())
            return NotFound($"No events found for intersection {intersectionId}");

        return Ok(events);
    }
}
