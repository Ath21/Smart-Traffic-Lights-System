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
            var issues = new List<string>();

            if (!await _lightCacheDbContext.CanConnectAsync())
                issues.Add("TrafficLightCacheDB Redis unreachable");
            
            if (!await _detectionCacheDbContext.CanConnectAsync())
                issues.Add("DetectionCacheDB Redis unreachable");

            if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                issues.Add("RabbitMQ not connected");

            if (issues.Any())
                return StatusCode(503, new { status = "Not Ready", service = "Traffic Light Controller Service", issues });

            return Ok(new { status = "Ready", service = "Traffic Light Controller Service" });
        }

    }
}
