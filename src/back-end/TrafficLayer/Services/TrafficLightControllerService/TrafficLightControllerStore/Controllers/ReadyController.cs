using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCacheData;

namespace TrafficLightControllerStore.Controllers
{
    [ApiController]
    [Route("traffic_light_controller_service")]
    public class ReadyController : ControllerBase
    {
        private readonly TrafficLightCacheDbContext _dbcontext;
        private readonly IBusControl _bus;

        public ReadyController(TrafficLightCacheDbContext dbcontext, IBusControl bus)
        {
            _dbcontext = dbcontext;
            _bus = bus;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            var issues = new List<string>();

            if (!await _dbcontext.CanConnectAsync())
                issues.Add("TrafficLightCacheDB Redis unreachable");

            if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                issues.Add("RabbitMQ not connected");

            if (issues.Any())
                return StatusCode(503, new { status = "Not Ready", service = "Traffic Light Controller Service", issues });

            return Ok(new { status = "Ready", service = "Traffic Light Controller Service" });
        }
    }
}
