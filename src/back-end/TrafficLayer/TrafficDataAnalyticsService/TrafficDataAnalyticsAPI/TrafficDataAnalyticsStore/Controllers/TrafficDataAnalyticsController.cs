/*
GET /summary/daily/{intersectionId}?date=2025-07-05
Returns one record from daily_summaries.

GET /summary/range/{intersectionId}?from=2025-07-01&to=2025-07-06
Returns trend data for dashboard graphs.

GET /alerts/recent?severity=HIGH&limit=10
Returns recent congestion alerts.

GET /counts/vehicle/{intersectionId}?from=...&to=...
Returns raw count data from vehicle_counts.
*/
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TrafficDataAnalyticsStore.Models;
using TrafficDataAnalyticsStore.Repository.Summary;


namespace TrafficDataAnalyticsStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficDataAnalyticsController : ControllerBase
    {
        private readonly IDailySummaryRepository _dailySummaryRepository;
        private readonly IMapper _mapper;

        public TrafficDataAnalyticsController(IDailySummaryRepository dailySummaryRepository, IMapper mapper)
        {
            _dailySummaryRepository = dailySummaryRepository;
            _mapper = mapper;
        }

        [HttpGet("summary/daily/{intersectionId}")]
        public async Task<ActionResult<DailySummaryDto>> GetDailySummary(string intersectionId, [FromQuery] DateTime date)
        {
            var summary = await _dailySummaryRepository.GetByIntersectionAndDateAsync(intersectionId, date);
            if (summary == null) return NotFound();

            return Ok(_mapper.Map<DailySummaryDto>(summary));
        }
    }
}
