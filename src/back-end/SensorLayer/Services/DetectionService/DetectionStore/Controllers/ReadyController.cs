using DetectionCacheData;
using DetectionData;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DetectionStore.Controllers
{
    [ApiController]
    [Route("detection_service")]
    public class ReadyController : ControllerBase
    {
        private readonly DetectionDbContext _detectionDbContext;
        private readonly DetectionCacheDbContext _detectionCacheDbContext;
        private readonly IBusControl _bus;

        public ReadyController(DetectionDbContext detectionDbContext, DetectionCacheDbContext detectionCacheDbContext, IBusControl bus)
        {
            _detectionDbContext = detectionDbContext;
            _detectionCacheDbContext = detectionCacheDbContext;
            _bus = bus;
        }

        [HttpGet("/ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _detectionDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "DetectionDB MongoDB unreachable" });

                if (!await _detectionCacheDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "DetectionCacheDB Redis unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "Traffic Analytics Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}