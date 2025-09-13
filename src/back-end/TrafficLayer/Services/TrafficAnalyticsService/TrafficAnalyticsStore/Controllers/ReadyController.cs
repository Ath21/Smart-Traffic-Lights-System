using DetectionCacheData;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficAnalyticsData;

namespace TrafficAnalyticsStore.Controllers
{
    [ApiController]
    [Route("traffic_analytics_service")]
    public class ReadyController : ControllerBase
    {
        private readonly TrafficAnalyticsDbContext _dbContext;
        private readonly DetectionCacheDbContext _detectionCacheDbContext;
        private readonly IBusControl _bus;

        public ReadyController(TrafficAnalyticsDbContext dbContext, DetectionCacheDbContext detectionCacheDbContext, IBusControl bus)
        {
            _dbContext = dbContext;
            _detectionCacheDbContext = detectionCacheDbContext;
            _bus = bus;
        }

        [HttpGet("/ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _dbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "TrafficAnalyticsDB PostgreSQL unreachable" });

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
