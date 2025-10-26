using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLightCacheData.Repositories;
using TrafficLightControllerStore.Domain;

namespace TrafficLightControllerStore.Controllers
{
    [ApiController]
    [Route("api/traffic-lights")]
    public class TrafficLightsController : ControllerBase
    {
        private readonly ITrafficLightCacheRepository _repository;
        private readonly TrafficLightContext _trafficLight;
        private readonly IntersectionContext _intersection;

        public TrafficLightsController(
            ITrafficLightCacheRepository repository,
            TrafficLightContext trafficLight,
            IntersectionContext intersection)
        {
            _repository = repository;
            _trafficLight = trafficLight;
            _intersection = intersection;
        }

        // GET api/traffic-lights/state
        [HttpGet("state")]
        public async Task<IActionResult> GetState()
        {
            int intersectionId = _intersection.Id;
            int lightId = _trafficLight.Id;

            var phase = await _repository.GetCurrentPhaseAsync(intersectionId, lightId);
            var remaining = await _repository.GetRemainingTimeAsync(intersectionId, lightId);
            var mode = await _repository.GetModeAsync(intersectionId, lightId);
            var priority = await _repository.GetPriorityAsync(intersectionId, lightId);
            var lastUpdate = await _repository.GetLastUpdateAsync(intersectionId, lightId);

            return Ok(new
            {
                IntersectionId = intersectionId,
                LightId = lightId,
                Phase = phase,
                Remaining = remaining,
                Mode = mode,
                Priority = priority,
                LastUpdate = lastUpdate
            });
        }

        // GET api/traffic-lights/cycle
        [HttpGet("cycle")]
        public async Task<IActionResult> GetCycle()
        {
            int intersectionId = _intersection.Id;
            int lightId = _trafficLight.Id;

            var cycleDuration = await _repository.GetCycleDurationAsync(intersectionId, lightId);
            var offset = await _repository.GetOffsetAsync(intersectionId, lightId);
            var localOffset = await _repository.GetLocalOffsetAsync(intersectionId, lightId);
            var cycleProgress = await _repository.GetCycleProgressAsync(intersectionId, lightId);

            return Ok(new
            {
                IntersectionId = intersectionId,
                LightId = lightId,
                CycleDuration = cycleDuration,
                Offset = offset,
                LocalOffset = localOffset,
                CycleProgress = cycleProgress
            });
        }

        // GET api/traffic-lights/failover
        [HttpGet("failover")]
        public async Task<IActionResult> GetFailover()
        {
            int intersectionId = _intersection.Id;
            int lightId = _trafficLight.Id;

            var failover = await _repository.GetFailoverAsync(intersectionId, lightId);
            return Ok(new { IntersectionId = intersectionId, LightId = lightId, Failover = failover });
        }
    }
}