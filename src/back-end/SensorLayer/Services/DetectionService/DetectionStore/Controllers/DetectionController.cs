using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DetectionStore.Business;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Controllers;

// ============================================================
// Sensor Layer / Detection Service - Detection Events
//
// Handles detection of vehicles, incidents, and anomalies.
// ===========================================================

[ApiController]
[Route("api/detections")]
public class DetectionController : ControllerBase
{
    private readonly IDetectionEventService _business;
    private readonly IMapper _mapper;

    public DetectionController(IDetectionEventService business, IMapper mapper)
    {
        _business = business;
        _mapper = mapper;
    }

    // ============================================================
    // GET: api/detection/{intersectionId}
    // Role: Anonymous
    // Description: Get active detection events for an intersection.
    // ============================================================
    [HttpGet("{intersectionId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveEvents(int intersectionId)
    {
        var events = await _business.GetActiveEventsAsync(intersectionId);
        return Ok(events);
    }

    // ============================================================
    // POST: api/detection/event
    // Role: TrafficOperator, Admin
    // Description: Report a new detection event.
    // ============================================================
    [HttpPost("event")]
    //[Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> ReportEvent([FromBody] DetectionEventRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _business.ReportEventAsync(request);
        return Ok(result);
    }
}
