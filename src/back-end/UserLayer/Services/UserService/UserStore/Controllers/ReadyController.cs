using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserData;

namespace UserStore.Controllers
{
    [ApiController]
    [Route("user_service")]
    public class ReadyController : ControllerBase
    {
        private readonly UserDbContext _dbContext;
        private readonly IBusControl _bus;

        public ReadyController(UserDbContext dbContext, IBusControl bus)
        {
            _dbContext = dbContext;
            _bus = bus;
        }

        [HttpGet("/ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _dbContext.CanConnectAsync())
                    return StatusCode(503, new { status = "Not Ready", reason = "UserDB MSSQL unreachable" });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessages.AuditLogMessage), out _))
                    return StatusCode(503, new { status = "Not Ready", reason = "RabbitMQ not connected" });

                return Ok(new { status = "Ready", service = "User Service" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Not Ready", error = ex.Message });
            }
        }
    }
}
