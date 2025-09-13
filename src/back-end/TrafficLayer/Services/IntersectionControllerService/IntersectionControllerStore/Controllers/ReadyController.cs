using DetectionCacheData;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCacheData;

namespace IntersectionControllerStore.Controllers
{
    [ApiController]
    [Route("intersection_controller_service")]
    public class ReadyController : ControllerBase
    {
        private readonly TrafficLightCacheDbContext _lightCacheDbContext;
        private readonly DetectionCacheDbContext _detectionCacheDbContext;
        private readonly IBusControl _bus;

        public ReadyController(TrafficLightCacheDbContext lightCacheDbContext, DetectionCacheDbContext detectionCacheDbContext, IBusControl bus)
        {
            _lightCacheDbContext = lightCacheDbContext;
            _detectionCacheDbContext = detectionCacheDbContext;
            _bus = bus;
        }

        [HttpGet("/ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _lightCacheDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "TrafficLightCacheDB Redis unreachable" });

                if (!await _detectionCacheDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "DetectionCacheDB Redis unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "Intersection Controller Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}
