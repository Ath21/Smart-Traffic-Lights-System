using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficLightData;
using TrafficLightCacheData;
using Messages.Log;

namespace TrafficLightCoordinatorStore.Controllers
{
    [ApiController]
    [Route("traffic_light_coordinator_service")]
    public class ReadyController : ControllerBase
    {
        private readonly TrafficLightDbContext _dbContext;
        private readonly TrafficLightCacheDbContext _cacheDbContext;
        private readonly IBusControl _bus;

        public ReadyController(
            TrafficLightDbContext dbContext,
            TrafficLightCacheDbContext cacheDbContext,
            IBusControl bus)
        {
            _dbContext = dbContext;
            _cacheDbContext = cacheDbContext;
            _bus = bus;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _dbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "TrafficLightDB MSSQL unreachable" });

                if (!await _cacheDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "TrafficLightCacheDB Redis unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "Traffic Light Coordinator Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}
