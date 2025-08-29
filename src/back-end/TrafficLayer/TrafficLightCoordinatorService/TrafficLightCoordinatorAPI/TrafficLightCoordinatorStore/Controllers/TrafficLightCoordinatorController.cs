using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCoordinatorStore.Business;
using TrafficLightCoordinatorStore.Models.Requests;
using TrafficLightCoordinatorStore.Models.Responses;
using TrafficLightCoordinatorStore.Models.Dtos;
using AutoMapper;
using TrafficLightCoordinatorStore.Business.Coordination;

namespace TrafficLightCoordinatorStore.Controllers;

[ApiController]
[Route("api/traffic/coordinator")]
public class TrafficLightCoordinatorController : ControllerBase
{
    private readonly ICoordinatorService _service;
    private readonly IMapper _mapper;

    public TrafficLightCoordinatorController(ICoordinatorService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // ============================================================
    // GET: /api/traffic/coordinator/config-read/{intersectionId}
    // Roles: User, TrafficOperator, Admin
    // Purpose: Fetch the latest traffic light configuration
    // ============================================================
    [HttpGet("config-read/{intersectionId:guid}")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(ConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfig(Guid intersectionId, CancellationToken ct)
    {
        var dto = await _service.GetConfigAsync(intersectionId, ct);
        if (dto is null) return NotFound();
        return Ok(_mapper.Map<ConfigResponse>(dto));
    }

    // ============================================================
    // POST: /api/traffic/coordinator/config-create/{intersectionId}
    // Roles: Admin
    // Purpose: Upload or update a traffic light configuration pattern
    // ============================================================
    [HttpPost("config-create/{intersectionId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertConfig(Guid intersectionId, [FromBody] UpdateConfigRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body?.Pattern))
            return BadRequest(new { error = "pattern is required" });

        var dto = await _service.UpsertConfigAsync(intersectionId, body.Pattern, ct);
        return Ok(_mapper.Map<ConfigResponse>(dto));
    }

    // ============================================================
    // POST: /api/traffic/coordinator/priority/override
    // Roles: TrafficOperator, Admin
    // Purpose: Manually override priority (emergency, pedestrian, cyclist, etc.)
    // ============================================================
    [HttpPost("priority/override")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(PriorityOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OverridePriority([FromBody] PriorityOverrideRequest body, CancellationToken ct)
    {
        if (body is null || body.IntersectionId == Guid.Empty || string.IsNullOrWhiteSpace(body.Type))
            return BadRequest(new { error = "intersectionId and type are required" });

        var dto = new PriorityDto
        {
            IntersectionId = body.IntersectionId,
            PriorityType   = body.Type,
            Reason         = body.Reason,
            AppliedPattern = PatternBuilder.For(body.Type, active: true),
            AppliedAt      = DateTimeOffset.UtcNow
        };

        var result = await _service.HandlePriorityAsync(dto, ct);
        return Ok(_mapper.Map<PriorityOverrideResponse>(result));
    }
}
