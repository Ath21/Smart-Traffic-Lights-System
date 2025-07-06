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
using Microsoft.AspNetCore.Mvc;
using TrafficDataAnalyticsStore.Business.Congestion;
using TrafficDataAnalyticsStore.Business.DailySum;

namespace TrafficDataAnalyticsStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficDataAnalyticsController : ControllerBase
    {
        private readonly ICongestionAlertService _congestionService;
        private readonly ISummaryService _summaryService;

        public TrafficDataAnalyticsController(
            ICongestionAlertService congestionService,
            ISummaryService summaryService)
        {
            _congestionService = congestionService;
            _summaryService = summaryService;
        }

        [HttpGet("congestion/alerts")]
        public async Task<IActionResult> GetActiveCongestionAlerts()
        {
            var alerts = await _congestionService.GetActiveAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("daily/summaries")]
        public async Task<IActionResult> GetLatestDailySummaries()
        {
            var summaries = await _summaryService.GetLatestSummariesAsync();
            return Ok(summaries);
        }
    }
}
