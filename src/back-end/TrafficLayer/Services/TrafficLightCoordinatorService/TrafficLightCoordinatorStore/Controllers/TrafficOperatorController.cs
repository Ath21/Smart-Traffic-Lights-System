using Microsoft.AspNetCore.Mvc;
using TrafficLightCoordinatorStore.Business.Operator;
using TrafficLightCoordinatorStore.Models;

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
}