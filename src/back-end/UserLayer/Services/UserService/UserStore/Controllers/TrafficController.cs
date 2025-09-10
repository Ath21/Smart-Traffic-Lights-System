using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserStore.Business.Traffic;
using UserStore.Models.Requests;

namespace UserStore.Controllers;

[ApiController]
[Route("api/traffic")]
public class TrafficController : ControllerBase
{
    private readonly ITrafficService _trafficService;
    private readonly ILogger<TrafficController> _logger;

    private const string ServiceTag = "[" + nameof(TrafficController) + "]";

    public TrafficController(ITrafficService trafficService, ILogger<TrafficController> logger)
    {
        _trafficService = trafficService;
        _logger = logger;
    }

    // ============================================================
    // POST: /api/traffic/{intersectionId}/lights/{lightId}/control
    // Roles: TrafficOperator, Admin
    // Purpose: Manually control a specific traffic light (send command)
    // ============================================================
    [HttpPost("{intersectionId:guid}/lights/{lightId:guid}/control")]
    //[Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ControlLight(Guid intersectionId, Guid lightId, [FromBody] ControlLightRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewState))
            return BadRequest(new { error = "NewState is required." });

        await _trafficService.ControlLightAsync(intersectionId, lightId, request.NewState);

        _logger.LogInformation(
            "{Tag} Control endpoint called for {IntersectionId}-{LightId} -> {State}",
            ServiceTag, intersectionId, lightId, request.NewState
        );

        return Ok(new
        {
            intersectionId,
            lightId,
            state = request.NewState,
            message = "Control command sent"
        });
    }

    // ============================================================
    // POST: /api/traffic/{intersectionId}/lights/{lightId}/update
    // Roles: TrafficOperator, Admin
    // Purpose: Publish an update for a traffic light (state synchronization)
    // ============================================================
    [HttpPost("{intersectionId:guid}/lights/{lightId:guid}/update")]
    //[Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLight(Guid intersectionId, Guid lightId, [FromBody] UpdateLightRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentState))
            return BadRequest(new { error = "CurrentState is required." });

        await _trafficService.UpdateLightAsync(intersectionId, lightId, request.CurrentState);

        _logger.LogInformation(
            "{Tag} Update endpoint called for {IntersectionId}-{LightId} -> {State}",
            ServiceTag, intersectionId, lightId, request.CurrentState
        );

        return Ok(new
        {
            intersectionId,
            lightId,
            state = request.CurrentState,
            message = "Update published"
        });
    }
}
