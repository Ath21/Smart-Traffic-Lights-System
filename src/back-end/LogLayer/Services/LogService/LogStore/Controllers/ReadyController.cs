using LogData;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LogStore.Controllers
{
    [ApiController]
    [Route("log_service")]
    public class ReadyController : ControllerBase
    {
        private readonly LogDbContext _logDbContext;
        private readonly IBusControl _bus;

        public ReadyController(LogDbContext logDbContext, IBusControl bus)
        {
            _logDbContext = logDbContext;
            _bus = bus;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _logDbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "LogDB MongoDB unreachable" });
                
                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "Log Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}