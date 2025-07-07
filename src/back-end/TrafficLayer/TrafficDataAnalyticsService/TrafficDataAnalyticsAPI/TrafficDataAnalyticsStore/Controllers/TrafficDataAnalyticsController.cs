using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TrafficDataAnalyticsStore.Models;
using TrafficDataAnalyticsStore.Repository.Congestion;
using TrafficDataAnalyticsStore.Repository.Summary;
using TrafficDataAnalyticsStore.Repository.Vehicle;


namespace TrafficDataAnalyticsStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficDataAnalyticsController : ControllerBase
    {
        private readonly IDailySummaryRepository _dailySummaryRepository;
        private readonly ICongestionAlertRepository _alertRepository;
        private readonly IVehicleCountRepository _vehicleCountRepository;
        private readonly IMapper _mapper;

        public TrafficDataAnalyticsController(
            IDailySummaryRepository dailySummaryRepository,
            ICongestionAlertRepository alertRepository,
            IVehicleCountRepository vehicleCountRepository,
            IMapper mapper)
        {
            _dailySummaryRepository = dailySummaryRepository;
            _alertRepository = alertRepository;
            _vehicleCountRepository = vehicleCountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// GET /summary/daily/{intersectionId}?date=YYYY-MM-DD
        /// </summary>
        [HttpGet("summary/daily/{intersectionId}")]
        public async Task<ActionResult<DailySummaryDto>> GetDailySummary(string intersectionId, [FromQuery] DateTime date)
        {
            var summary = await _dailySummaryRepository.GetByIntersectionAndDateAsync(intersectionId, date);
            if (summary == null) return NotFound();

            return Ok(_mapper.Map<DailySummaryDto>(summary));
        }

        /// <summary>
        /// GET /summary/range/{intersectionId}?from=YYYY-MM-DD&to=YYYY-MM-DD
        /// </summary>
        [HttpGet("summary/range/{intersectionId}")]
        public async Task<ActionResult<IEnumerable<DailySummaryDto>>> GetRangeSummary(string intersectionId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var summaries = await _dailySummaryRepository.GetRangeByIntersectionAsync(intersectionId, from, to);
            var result = summaries.Select(_mapper.Map<DailySummaryDto>);
            return Ok(result);
        }

        /// <summary>
        /// GET /alerts/recent?severity=HIGH&limit=10
        /// </summary>
        [HttpGet("alerts/recent")]
        public async Task<ActionResult<IEnumerable<CongestionAlertDto>>> GetRecentAlerts([FromQuery] string severity = "HIGH", [FromQuery] int limit = 10)
        {
            var alerts = await _alertRepository.GetRecentAlertsAsync(severity, limit);
            var result = alerts.Select(a => new CongestionAlertDto
            {
                IntersectionId = a.IntersectionId,
                Severity = a.Severity
            });
            return Ok(result);
        }

        /// <summary>
        /// GET /counts/vehicle/{intersectionId}?from=YYYY-MM-DD&to=YYYY-MM-DD
        /// </summary>
        [HttpGet("counts/vehicle/{intersectionId}")]
        public async Task<ActionResult<IEnumerable<VehicleCountDto>>> GetVehicleCounts(string intersectionId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var counts = await _vehicleCountRepository.GetRangeByIntersectionAsync(intersectionId, from, to);
            var result = counts.Select(c => new VehicleCountDto
            {
                IntersectionId = c.IntersectionId,
                Timestamp = c.Timestamp,
                Count = c.Count,
                LaneId = c.LaneId
            });
            return Ok(result);
        }
    }
}
