// Controllers/TrafficCoordinationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCoordinatorStore.Business.Coordination;
using TrafficLightCoordinatorStore.Models;

namespace TrafficLightCoordinatorStore.Controllers;

[ApiController]
[Route("traffic-coordination/{intersection_id:guid}/schedule")]
public class TrafficCoordinationController : ControllerBase
{
    private readonly IScheduleService _service;

    public TrafficCoordinationController(IScheduleService service) => _service = service;

    /// GET /traffic-coordination/{intersection_id}/schedule
    /// Roles: Operator, Admin
    [HttpGet]
    //[Authorize(Roles = "Operator,Admin")]
    [ProducesResponseType(typeof(GetScheduleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(
        [FromRoute(Name = "intersection_id")] Guid intersectionId,
        CancellationToken ct)
    {
        var dto = await _service.GetScheduleAsync(intersectionId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// POST /traffic-coordination/{intersection_id}/schedule
    /// Roles: Admin
    [HttpPost]
    //[Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UpdateScheduleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSchedule(
        [FromRoute(Name = "intersection_id")] Guid intersectionId,
        [FromBody] UpdateScheduleRequestDto body,
        CancellationToken ct)
    {
        if (body?.Schedule_Pattern?.Phases is null || body.Schedule_Pattern.Phases.Count == 0)
            return BadRequest(new { error = "schedule_pattern.phases is required and cannot be empty" });

        await _service.UpdateScheduleAsync(intersectionId, body.Schedule_Pattern.Phases, ct);

        return Ok(new UpdateScheduleResponseDto(
            Success: true,
            Message: "Traffic light schedule updated."
        ));
    }
}
