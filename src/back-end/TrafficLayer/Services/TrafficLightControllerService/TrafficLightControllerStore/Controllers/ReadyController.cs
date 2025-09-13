using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCacheData;

namespace TrafficLightControllerStore.Controllers
{
    [ApiController]
    [Route("notification_service")]
    public class ReadyController : ControllerBase
    {
        private readonly TrafficLightCacheDbContext _dbcontext;
        private readonly IBusControl _bus;

        public ReadyController(TrafficLightCacheDbContext dbcontext, IBusControl bus)
        {
            _dbcontext = dbcontext;
            _bus = bus;
        }

        [HttpGet("/ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _dbcontext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "TrafficLightCacheDB Redis unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "Traffic Light Controller Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}
