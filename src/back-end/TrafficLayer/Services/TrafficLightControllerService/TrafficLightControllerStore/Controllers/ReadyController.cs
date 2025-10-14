using MassTransit;
using Messages.Log;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCacheData;
using TrafficLightControllerStore.Domain;

namespace TrafficLightControllerStore.Controllers;

[ApiController]
[Route("traffic_light_controller_service")]
public class ReadyController : ControllerBase
{
    private readonly TrafficLightCacheDbContext _trafficLightCacheDbContext;
    private readonly IBusControl _bus;
    private readonly IntersectionContext _intersection;
    private readonly TrafficLightContext _light;

    public ReadyController(
        TrafficLightCacheDbContext trafficLightCacheDbContext,
        IBusControl bus,
        IntersectionContext intersection,
        TrafficLightContext light)
    {
        _trafficLightCacheDbContext = trafficLightCacheDbContext;
        _bus = bus;
        _intersection = intersection;
        _light = light;
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            if (!await _trafficLightCacheDbContext.CanConnectAsync())
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    reason = "TrafficLightCacheDB (Redis) unreachable",
                    intersection = _intersection,
                    light = _light
                });

            if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    reason = "RabbitMQ not connected",
                    intersection = _intersection,
                    light = _light
                });

            return Ok(new
            {
                status = "Ready",
                service = "Traffic Light Controller Service",
                intersection = new
                {
                    id = _intersection.Id,
                    name = _intersection.Name
                },
                light = new
                {
                    id = _light.Id,
                    name = _light.Name
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Not Ready",
                error = ex.Message,
                intersection = _intersection,
                light = _light
            });
        }
    }
}
