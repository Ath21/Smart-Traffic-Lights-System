using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Models.Requests;
using TrafficLightControllerStore.Models.Responses;

namespace TrafficLightControllerStore.Controllers;

// ============================================================
// Traffic Layer / Traffic Light Controller Service - Direct Control
//
// Handles real-time state management of traffic lights.
// ============================================================

[ApiController]
[Route("api/traffic/controller")]
public class TrafficLightController : ControllerBase
{
    private readonly ITrafficLightControlService _service;
    private readonly IMapper _mapper;
    private readonly ILogger<TrafficLightController> _logger;

    private const string ServiceTag = "[" + nameof(TrafficLightController) + "]";

    public TrafficLightController(
        ITrafficLightControlService service,
        IMapper mapper,
        ILogger<TrafficLightController> logger)
    {
        _service = service;
        _mapper = mapper;
        _logger = logger;
    }

    // ============================================================
    // POST: /api/traffic/controller/lights/{intersection}/{light}/state
    // Roles: TrafficOperator, Admin
    // Purpose: Manual override -> update Redis + publish to RabbitMQ
    // ============================================================
    [HttpPost("lights/{intersection}/{light}/state")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(TrafficLightStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForceStateChange(
        string intersection,
        string light,
        [FromBody] UpdateLightRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentState))
            return BadRequest(new { error = "CurrentState is required." });

        var dto = await _service.ForceStateChangeAsync(intersection, light, request.CurrentState, request.Duration, request.Reason);

        _logger.LogInformation(
            "{Tag} POST override applied: Intersection={Intersection}, Light={Light}, State={State}, Duration={Duration}, Reason={Reason}",
            ServiceTag, intersection, light, request.CurrentState, request.Duration, request.Reason);

        var response = _mapper.Map<TrafficLightStatusResponse>(dto);
        return Ok(response);
    }

    // ============================================================
    // GET: /api/traffic/controller/lights/{intersection}
    // Roles: User, TrafficOperator, Admin
    // Purpose: Retrieve current state of lights at an intersection
    // ============================================================
    [HttpGet("lights/{intersection}")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<TrafficLightStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(string intersection)
    {
        _logger.LogInformation("{Tag} GET states for intersection {Intersection}", ServiceTag, intersection);

        var dtos = await _service.GetCurrentStatesAsync(intersection);
        var response = _mapper.Map<IEnumerable<TrafficLightStatusResponse>>(dtos);

        return Ok(response);
    }

    // ============================================================
    // GET: /api/traffic/controller/events/{intersection}
    // Roles: TrafficOperator, Admin
    // Purpose: Retrieve last applied control events
    // ============================================================
    [HttpGet("events/{intersection}")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<ControlEventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(string intersection)
    {
        _logger.LogInformation("{Tag} GET events for intersection {Intersection}", ServiceTag, intersection);

        var dtos = await _service.GetLastControlEventsAsync(intersection);
        var response = _mapper.Map<IEnumerable<ControlEventResponse>>(dtos);

        return Ok(response);
    }
}
