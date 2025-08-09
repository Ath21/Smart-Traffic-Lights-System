using Microsoft.AspNetCore.Mvc;
using PublicTransportDetectionStore.Business;
using PublicTransportDetectionStore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PublicTransportDetectionStore.Controllers
{
    [ApiController]
    [Route("sensor-detections/public-transport")]
    public class PublicTransportDetectionController : ControllerBase
    {
        private readonly IPublicTransportDetectService _detectionService;
        private readonly ILogger<PublicTransportDetectionController> _logger;

        public PublicTransportDetectionController(
            IPublicTransportDetectService detectionService,
            ILogger<PublicTransportDetectionController> logger)
        {
            _detectionService = detectionService;
            _logger = logger;
        }

        /// <summary>
        /// Ingest new public transport detection data
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDetection([FromBody] PublicTransportDetectionCreateDto dto)
        {
            var detectionId = await _detectionService.AddDetectionAsync(dto);
            return Ok(detectionId);
        }

        /// <summary>
        /// Query public transport detections by intersection, route, and/or time range
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PublicTransportDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetections(
            [FromQuery] Guid? intersection_id,
            [FromQuery] string? route_id,
            [FromQuery] DateTime? start_time,
            [FromQuery] DateTime? end_time,
            [FromQuery] int? limit)
        {
            var results = await _detectionService.GetDetectionsAsync(intersection_id, route_id, start_time, end_time, limit);
            return Ok(results);
        }

        /// <summary>
        /// Get all public transport detections (no filters)
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<PublicTransportDetectionReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDetections()
        {
            var results = await _detectionService.GetDetectionsAsync(null, null, null, null, null);
            return Ok(results);
        }
    }
}
