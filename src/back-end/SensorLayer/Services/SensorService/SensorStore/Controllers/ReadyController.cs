using DetectionCacheData;
using DetectionData;
using MassTransit;
using Messages.Log;
using Microsoft.AspNetCore.Mvc;
using SensorStore.Domain;

namespace SensorStore.Controllers;

[ApiController]
[Route("api/sensors")]
public class ReadyController : ControllerBase
{
    private readonly DetectionDbContext _detectionDbContext;
    private readonly DetectionCacheDbContext _detectionCacheDbContext;
    private readonly IBusControl _bus;
    private readonly IntersectionContext _intersection;

    public ReadyController(
        DetectionDbContext detectionDbContext,
        DetectionCacheDbContext detectionCacheDbContext,
        IBusControl bus,
        IntersectionContext intersection)
    {
        _detectionDbContext = detectionDbContext;
        _detectionCacheDbContext = detectionCacheDbContext;
        _bus = bus;
        _intersection = intersection;
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            if (!await _detectionDbContext.CanConnectAsync())
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    reason = "DetectionDB MongoDB unreachable",
                    intersection = new { _intersection.Id, _intersection.Name }
                });

            if (!await _detectionCacheDbContext.CanConnectAsync())
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    reason = "DetectionCacheDB Redis unreachable",
                    intersection = new { _intersection.Id, _intersection.Name }
                });

            if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    reason = "RabbitMQ not connected",
                    intersection = new { _intersection.Id, _intersection.Name }
                });

            return Ok(new
            {
                status = "Ready",
                service = "Sensor Service",
                intersection = new { _intersection.Id, _intersection.Name }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Not Ready",
                error = ex.Message,
                intersection = new { _intersection.Id, _intersection.Name }
            });
        }
    }
}
