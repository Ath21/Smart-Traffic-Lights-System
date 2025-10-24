using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficLightData;
using TrafficLightCacheData;
using Messages.Log;
using System.Net;
using System.Net.Sockets;

namespace TrafficLightCoordinatorStore.Controllers
{
    [ApiController]
    [Route("traffic-light-coordinator")]
    public class ReadyController : ControllerBase
    {
        private readonly TrafficLightDbContext _dbContext;
        private readonly TrafficLightCacheDbContext _cacheDbContext;
        private readonly IBusControl _bus;

        private readonly string _service;
        private readonly string _layer;
        private readonly string _level;
        private readonly string _environment;
        private readonly string _hostname;
        private readonly string _containerIp;

        public ReadyController(
            TrafficLightDbContext dbContext,
            TrafficLightCacheDbContext cacheDbContext,
            IBusControl bus)
        {
            _dbContext = dbContext;
            _cacheDbContext = cacheDbContext;
            _bus = bus;

            _service = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Traffic Light Coordinator";
            _layer = Environment.GetEnvironmentVariable("SERVICE_LAYER") ?? "Traffic";
            _level = Environment.GetEnvironmentVariable("SERVICE_LEVEL") ?? "Cloud";
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _hostname = Environment.MachineName;
            _containerIp = Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString() ?? "unknown";
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            var status = new Dictionary<string, object?>
            {
                ["status"] = "Ready",
                ["service"] = _service,
                ["layer"] = _layer,
                ["level"] = _level,
                ["environment"] = _environment,
                ["hostname"] = _hostname,
                ["container_ip"] = _containerIp,
                ["timestamp"] = DateTime.UtcNow.ToString("u")
            };

            try
            {
                // ---- TrafficLight MSSQL ----
                bool dbConnected = await _dbContext.CanConnectAsync();
                status["traffic_light_db"] = new { name = "TrafficLightDB (MSSQL)", reachable = dbConnected };
                if (!dbConnected)
                {
                    status["status"] = "Not Ready";
                    status["reason"] = "TrafficLightDB MSSQL unreachable";
                    return StatusCode(503, status);
                }

                // ---- Cache Redis ----
                bool cacheConnected = await _cacheDbContext.CanConnectAsync();
                status["cache_db"] = new { name = "TrafficLightCacheDB (Redis)", reachable = cacheConnected };
                if (!cacheConnected)
                {
                    status["status"] = "Not Ready";
                    status["reason"] = "TrafficLightCacheDB Redis unreachable";
                    return StatusCode(503, status);
                }

                // ---- RabbitMQ ----
                bool brokerConnected = _bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _);
                status["message_broker"] = new { name = "RabbitMQ", reachable = brokerConnected };
                if (!brokerConnected)
                {
                    status["status"] = "Not Ready";
                    status["reason"] = "RabbitMQ not connected";
                    return StatusCode(503, status);
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                status["status"] = "Not Ready";
                status["error"] = ex.Message;
                return StatusCode(503, status);
            }
        }
    }
}
