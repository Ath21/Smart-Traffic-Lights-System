using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehicleDetectionStore.Business;
using VehicleDetectionStore.Models;

namespace VehicleDetectionStore.Controllers
{
    [ApiController]
    [Route("sensor-detections/vehicle")]
    public class VehicleDetectionController : ControllerBase
    {
        private readonly IVehicleDetectService _detectionService;
        private readonly ILogger<VehicleDetectionController> _logger;

        public VehicleDetectionController(
            IVehicleDetectService detectionService,
            ILogger<VehicleDetectionController> logger)
        {
            _detectionService = detectionService;
            _logger = logger;
        }

        /// <summary>
        /// Ingest new vehicle detection data
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(DetectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDetection([FromBody] VehicleDetectionCreateDto dto)
        {
            var result = await _detectionService.AddDetectionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Query vehicle detections by intersection and/or time range
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VehicleDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetections(
            [FromQuery] Guid? intersection_id,
            [FromQuery] DateTime? start_time,
            [FromQuery] DateTime? end_time,
            [FromQuery] int? limit)
        {
            var results = await _detectionService.GetDetectionsAsync(intersection_id, start_time, end_time, limit);
            return Ok(results);
        }
    }
}
