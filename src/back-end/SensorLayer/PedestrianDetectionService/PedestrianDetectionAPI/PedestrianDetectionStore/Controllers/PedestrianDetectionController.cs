using Microsoft.AspNetCore.Mvc;
using PedestrianDetectionStore.Business;
using PedestrianDetectionStore.Models;

namespace PedestrianDetectionStore.Controllers
{
    [ApiController]
    [Route("sensor-detections/pedestrian")]
    public class PedestrianDetectionController : ControllerBase
    {
        private readonly IPedestrianDetectService _detectionService;
        private readonly ILogger<PedestrianDetectionController> _logger;

        public PedestrianDetectionController(
            IPedestrianDetectService detectionService,
            ILogger<PedestrianDetectionController> logger)
        {
            _detectionService = detectionService;
            _logger = logger;
        }

        /// <summary>
        /// Ingest new pedestrian detection data
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PedestrianDetectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDetection([FromBody] PedestrianDetectionCreateDto dto)
        {
            var result = await _detectionService.AddDetectionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Query pedestrian detections by intersection and/or time range
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PedestrianDetectionReadDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(IEnumerable<PedestrianDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDetections()
        {
            // Pass null for all filters to retrieve everything
            var results = await _detectionService.GetDetectionsAsync(null, null, null, null);
            return Ok(results);
        }
    }
}
