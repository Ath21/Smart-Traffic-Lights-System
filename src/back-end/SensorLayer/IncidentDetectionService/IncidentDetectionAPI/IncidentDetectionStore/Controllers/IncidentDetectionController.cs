using Microsoft.AspNetCore.Mvc;
using IncidentDetectionStore.Business;
using IncidentDetectionStore.Models;

namespace IncidentDetectionStore.Controllers
{
    [ApiController]
    [Route("sensor-detections/incident")]
    public class IncidentDetectionController : ControllerBase
    {
        private readonly IIncidentDetectService _detectionService;
        private readonly ILogger<IncidentDetectionController> _logger;

        public IncidentDetectionController(
            IIncidentDetectService detectionService,
            ILogger<IncidentDetectionController> logger)
        {
            _detectionService = detectionService;
            _logger = logger;
        }

        /// <summary>
        /// Ingest new incident detection data
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IncidentDetectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDetection([FromBody] IncidentDetectionCreateDto dto)
        {
            var result = await _detectionService.AddDetectionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Query incident detections by intersection and/or time range
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IncidentDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetections(
            [FromQuery] Guid? intersection_id,
            [FromQuery] DateTime? start_time,
            [FromQuery] DateTime? end_time,
            [FromQuery] int? limit)
        {
            var results = await _detectionService.GetDetectionsAsync(intersection_id, start_time, end_time, limit);
            return Ok(results);
        }

        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<IncidentDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDetections()
        {
            var results = await _detectionService.GetDetectionsAsync(null, null, null, null);
            return Ok(results);
        }
    }
}
