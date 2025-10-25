using Microsoft.AspNetCore.Mvc;
using TrafficLightCoordinatorStore.Business.Operator;
using TrafficLightCoordinatorStore.Models.Requests;

namespace TrafficLightCoordinatorStore.Controllers;

[ApiController]
[Route("api/traffic-operator")]
public class TrafficOperatorController : ControllerBase
{
    private readonly ITrafficOperatorBusiness _service;
    private readonly ILogger<TrafficOperatorController> _logger;

    public TrafficOperatorController(
        ITrafficOperatorBusiness service,
        ILogger<TrafficOperatorController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("apply-mode")]
    public async Task<IActionResult> ApplyMode([FromBody] ApplyModeRequest request)
    {
        try
        {
            await _service.ApplyModeAsync(request.IntersectionId, request.Mode);
            return Ok(new { Message = $"Mode '{request.Mode}' applied to intersection {request.IntersectionId}" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Failed to apply mode");
            return NotFound(new { Error = ex.Message });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Unexpected error applying mode");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("override-light")]
    public async Task<IActionResult> OverrideLight([FromBody] OverrideLightRequest request)
    {
        try
        {
            await _service.OverrideLightAsync(
                request.IntersectionId,
                request.LightId,
                request.Mode,
                request.PhaseDurations,
                request.RemainingTimeSec,
                request.CycleDurationSec,
                request.LocalOffsetSec,
                request.CycleProgressSec,
                request.PriorityLevel,
                request.IsFailoverActive
            );

            return Ok(new
            {
                Message = $"Override applied to light {request.LightId} on intersection {request.IntersectionId} with mode '{request.Mode}'"
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Failed to override light");
            return NotFound(new { Error = ex.Message });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Unexpected error overriding light");
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}