using System.Net;
using System.Net.Sockets;
using MassTransit;
using Messages.Log;
using Microsoft.AspNetCore.Mvc;
using NotificationData;

namespace NotificationStore.Controllers
{
    [ApiController]
    [Route("notification-service")]
    public class ReadyController : ControllerBase
    {
        private readonly NotificationDbContext _dbContext;
        private readonly IBusControl _bus;

        private readonly string _service;
        private readonly string _environment;
        private readonly string _hostname;
        private readonly string _containerIp;

        public ReadyController(NotificationDbContext dbContext, IBusControl bus)
        {
            _dbContext = dbContext;
            _bus = bus;

            _service = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Notification Service";
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _hostname = Environment.MachineName;
            _containerIp = Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString() ?? "unknown";
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                var mongoOk = await _dbContext.CanConnectAsync();
                if (!mongoOk)
                {
                    return StatusCode(503, new
                    {
                        status = "Not Ready",
                        reason = "NotificationDB (MongoDB) unreachable",
                        service = _service,
                        environment = _environment,
                        hostname = _hostname,
                        container_ip = _containerIp
                    });
                }

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
                {
                    return StatusCode(503, new
                    {
                        status = "Not Ready",
                        reason = "RabbitMQ broker unreachable or topology not established",
                        service = _service,
                        environment = _environment,
                        hostname = _hostname,
                        container_ip = _containerIp
                    });
                }

                return Ok(new
                {
                    status = "Ready",
                    service = _service,
                    environment = _environment,
                    hostname = _hostname,
                    container_ip = _containerIp,
                    timestamp = DateTime.UtcNow.ToString("u")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    error = ex.Message,
                    service = _service,
                    environment = _environment,
                    hostname = _hostname,
                    container_ip = _containerIp
                });
            }
        }
    }
}
