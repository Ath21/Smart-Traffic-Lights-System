using Microsoft.AspNetCore.Mvc;
using CyclistDetectionStore.Business;
using CyclistDetectionStore.Models;

namespace CyclistDetectionStore.Controllers
{
    [ApiController]
    [Route("sensor-detections/cyclist")]
    public class CyclistDetectionController : ControllerBase
    {
        private readonly ICyclistDetectService _detectionService;
        private readonly ILogger<CyclistDetectionController> _logger;

        public CyclistDetectionController(
            ICyclistDetectService detectionService,
            ILogger<CyclistDetectionController> logger)
        {
            _detectionService = detectionService;
            _logger = logger;
        }

        /// <summary>
        /// Ingest new cyclist detection data
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CyclistDetectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDetection([FromBody] CyclistDetectionCreateDto dto)
        {
            var result = await _detectionService.CreateDetectionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Query cyclist detections by intersection and/or time range
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CyclistDetectionReadDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(IEnumerable<CyclistDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDetections()
        {
            var results = await _detectionService.GetDetectionsAsync(null, null, null, null);
            return Ok(results);
        }
    }
}
