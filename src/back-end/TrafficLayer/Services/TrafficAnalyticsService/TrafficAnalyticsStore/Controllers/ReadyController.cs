using DetectionData;
using MassTransit;
using Messages.Log;
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
        private readonly DetectionDbContext _detectionDbContext;
        private readonly IBusControl _bus;

        public ReadyController(TrafficAnalyticsDbContext dbContext, DetectionDbContext detectionDbContext, IBusControl bus)
        {
            _dbContext = dbContext;
            _detectionDbContext = detectionDbContext;
            _bus = bus;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _dbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "TrafficAnalyticsDB PostgreSQL unreachable" });

                if (!await _detectionDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "DetectionDB MongoDB unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
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
