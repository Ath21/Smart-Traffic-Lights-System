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

    // POST: api/traffic/{intersectionId}/lights/{lightId}/control
    [HttpPost("{intersectionId}/lights/{lightId}/control")]
    public async Task<IActionResult> ControlLight(Guid intersectionId, Guid lightId, [FromBody] ControlLightRequest request)
    {
        await _trafficService.ControlLightAsync(intersectionId, lightId, request.NewState);

        _logger.LogInformation(
            "{Tag} Control endpoint called for {IntersectionId}-{LightId} -> {State}",
            ServiceTag, intersectionId, lightId, request.NewState
        );

        return Ok(new { intersectionId, lightId, request.NewState, message = "Control command sent" });
    }

    // POST: api/traffic/{intersectionId}/lights/{lightId}/update
    [HttpPost("{intersectionId}/lights/{lightId}/update")]
    public async Task<IActionResult> UpdateLight(Guid intersectionId, Guid lightId, [FromBody] UpdateLightRequest request)
    {
        await _trafficService.UpdateLightAsync(intersectionId, lightId, request.CurrentState);

        _logger.LogInformation(
            "{Tag} Update endpoint called for {IntersectionId}-{LightId} -> {State}",
            ServiceTag, intersectionId, lightId, request.CurrentState
        );

        return Ok(new { intersectionId, lightId, request.CurrentState, message = "Update published" });
    }
}