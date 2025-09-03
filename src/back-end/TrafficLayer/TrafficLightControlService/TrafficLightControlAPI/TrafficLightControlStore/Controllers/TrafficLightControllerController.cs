using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficLightControlStore.Business;
using TrafficLightControlStore.Models.Requests;
using TrafficLightControlStore.Models.Responses;

namespace TrafficLightControlStore.Controllers;

[ApiController]
[Route("api/traffic/controller")]
public class TrafficLightsController : ControllerBase
{
    private readonly ITrafficLightControlService _service;
    private readonly IMapper _mapper;
    private readonly ILogger<TrafficLightsController> _logger;

    private const string ServiceTag = "[" + nameof(TrafficLightsController) + "]";

    public TrafficLightsController(
        ITrafficLightControlService service,
        IMapper mapper,
        ILogger<TrafficLightsController> logger)
    {
        _service = service;
        _mapper = mapper;
        _logger = logger;
    }

    // ============================================================
    // POST: /api/traffic/controller/lights/{intersectionId}/{lightId}/state
    // Roles: TrafficOperator, Admin
    // Purpose: Manual override -> update Redis + publish to RabbitMQ
    // ============================================================
    [HttpPost("lights/{intersectionId:guid}/{lightId:guid}/state")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(TrafficLightStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForceStateChange(
        Guid intersectionId,
        Guid lightId,
        [FromBody] UpdateLightRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentState))
            return BadRequest(new { error = "CurrentState is required." });

        var dto = await _service.ForceStateChangeAsync(intersectionId, lightId, request.CurrentState);

        _logger.LogInformation(
            "{Tag} POST override applied: Intersection={IntersectionId}, Light={LightId}, State={State}",
            ServiceTag, intersectionId, lightId, request.CurrentState);

        var response = _mapper.Map<TrafficLightStatusResponse>(dto);
        return Ok(response);
    }

    // ============================================================
    // GET: /api/traffic/controller/lights/{intersectionId}
    // Roles: User, TrafficOperator, Admin
    // Purpose: Retrieve current state of lights at an intersection
    // ============================================================
    [HttpGet("lights/{intersectionId:guid}")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<TrafficLightStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(Guid intersectionId)
    {
        _logger.LogInformation("{Tag} GET states for intersection {IntersectionId}", ServiceTag, intersectionId);

        var dtos = await _service.GetCurrentStatesAsync(intersectionId);
        var response = _mapper.Map<IEnumerable<TrafficLightStatusResponse>>(dtos);

        return Ok(response);
    }

    // ============================================================
    // GET: /api/traffic/controller/events/{intersectionId}
    // Roles: TrafficOperator, Admin
    // Purpose: Retrieve last applied control events
    // ============================================================
    [HttpGet("events/{intersectionId:guid}")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<ControlEventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(Guid intersectionId)
    {
        _logger.LogInformation("{Tag} GET events for intersection {IntersectionId}", ServiceTag, intersectionId);

        var dtos = await _service.GetLastControlEventsAsync(intersectionId);
        var response = _mapper.Map<IEnumerable<ControlEventResponse>>(dtos);

        return Ok(response);
    }
}
