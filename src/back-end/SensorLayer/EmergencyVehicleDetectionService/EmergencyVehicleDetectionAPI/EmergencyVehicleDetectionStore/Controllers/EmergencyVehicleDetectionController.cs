using Microsoft.AspNetCore.Mvc;
using EmergencyVehicleDetectionStore.Business;
using EmergencyVehicleDetectionStore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EmergencyVehicleDetectionStore.Controllers
{
    [ApiController]
    [Route("sensor-detections/emergency-vehicle")]
    public class EmergencyVehicleDetectionController : ControllerBase
    {
        private readonly IEmergencyVehicleDetectService _detectionService;
        private readonly ILogger<EmergencyVehicleDetectionController> _logger;

        public EmergencyVehicleDetectionController(
            IEmergencyVehicleDetectService detectionService,
            ILogger<EmergencyVehicleDetectionController> logger)
        {
            _detectionService = detectionService;
            _logger = logger;
        }

        /// <summary>
        /// Ingest new emergency vehicle detection data
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(DetectionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDetection([FromBody] EmergencyVehicleDetectionCreateDto dto)
        {
            var result = await _detectionService.AddDetectionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Query emergency vehicle detections by intersection and/or time range
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmergencyVehicleDetectionReadDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(IEnumerable<EmergencyVehicleDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDetections()
        {
            var results = await _detectionService.GetDetectionsAsync(null, null, null, null);
            return Ok(results);
        }
    }
}
