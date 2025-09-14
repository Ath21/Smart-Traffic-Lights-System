using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Failover;
using TrafficLightControllerStore.Models.Responses;

namespace TrafficLightControllerStore.Controllers;

[ApiController]
[Route("api/traffic/controller")]
public class FailoverController : ControllerBase
{
    private readonly ITrafficLightControlService _service;
    private readonly IFailoverService _failoverService;
    private readonly IMapper _mapper;
    private readonly ILogger<FailoverController> _logger;

    private const string ServiceTag = "[" + nameof(FailoverController) + "]";

    public FailoverController(
        ITrafficLightControlService service,
        IFailoverService failoverService,
        IMapper mapper,
        ILogger<FailoverController> logger)
    {
        _service = service;
        _failoverService = failoverService;
        _mapper = mapper;
        _logger = logger;
    }

    // ===============================
    // POST: /api/traffic/controller/failover/{intersection}/{light?}
    // Roles: TrafficOperator, Admin
    // Purpose: Force failover mode for one light or all lights in intersection
    // ===============================
    [HttpPost("failover/{intersection}/{light?}")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    [ProducesResponseType(typeof(IEnumerable<TrafficLightStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyFailover(string intersection, string? light = null)
    {
        if (!string.IsNullOrEmpty(light))
        {
            await _failoverService.ApplyFailoverAsync(intersection, light);
            _logger.LogWarning("{Tag} Manual failover applied: {Intersection}-{Light}", ServiceTag, intersection, light);

            return Ok(new
            {
                status = "FailoverApplied",
                intersection,
                light,
                state = "BlinkingYellow"
            });
        }
        else
        {
            await _failoverService.ApplyFailoverIntersectionAsync(intersection);
            _logger.LogWarning("{Tag} Manual failover applied for intersection: {Intersection}", ServiceTag, intersection);

            return Ok(new
            {
                status = "FailoverApplied",
                intersection,
                allLights = true,
                state = "BlinkingYellow"
            });
        }
    }
}
