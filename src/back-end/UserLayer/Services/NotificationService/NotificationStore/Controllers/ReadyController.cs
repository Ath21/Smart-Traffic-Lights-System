using MassTransit;
using Messages.Log;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using NotificationData;

namespace NotificationStore.Controllers
{
    [ApiController]
    [Route("notification_service")]
    public class ReadyController : ControllerBase
    {
        private readonly NotificationDbContext _dbcontext;
        private readonly IBusControl _bus;

        public ReadyController(NotificationDbContext dbcontext, IBusControl bus)
        {
            _dbcontext = dbcontext;
            _bus = bus;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _dbcontext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "NotificationDB MongoDB unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "Notification Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}
