using Microsoft.AspNetCore.Http;
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
