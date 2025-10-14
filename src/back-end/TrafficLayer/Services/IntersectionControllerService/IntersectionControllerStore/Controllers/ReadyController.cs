using DetectionCacheData;
using IntersectionControllerStore.Domain;
using MassTransit;
using Messages.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCacheData;

namespace IntersectionControllerStore.Controllers
{
    public class ReadyController : ControllerBase
    {
        private readonly TrafficLightCacheDbContext _trafficLightCacheDbContext;
        private readonly DetectionCacheDbContext _detectionCacheDbContext;
        private readonly IBusControl _bus;
        private readonly IntersectionContext _intersection;

        public ReadyController(
            TrafficLightCacheDbContext trafficLightCacheDbContext,
            DetectionCacheDbContext detectionCacheDbContext,
            IBusControl bus,
            IntersectionContext intersection)
        {
            _trafficLightCacheDbContext = trafficLightCacheDbContext;
            _detectionCacheDbContext = detectionCacheDbContext;
            _bus = bus;
            _intersection = intersection;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                if (!await _trafficLightCacheDbContext.CanConnectAsync())
                    return StatusCode(503, new
                    {
                        status = "Not Ready",
                        reason = "TrafficLightCacheDB (Redis) unreachable",
                        intersection = _intersection
                    });

                if (!await _detectionCacheDbContext.CanConnectAsync())
                    return StatusCode(503, new
                    {
                        status = "Not Ready",
                        reason = "DetectionCacheDB (Redis) unreachable",
                        intersection = _intersection
                    });

                if (!_bus.Topology.TryGetPublishAddress(typeof(LogMessage), out _))
                    return StatusCode(503, new
                    {
                        status = "Not Ready",
                        reason = "RabbitMQ not connected",
                        intersection = _intersection
                    });

                return Ok(new
                {
                    status = "Ready",
                    service = "Intersection Controller Service",
                    intersection = new
                    {
                        id = _intersection.Id,
                        name = _intersection.Name
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "Not Ready",
                    error = ex.Message,
                    intersection = _intersection
                });
            }
        }
    }
}
