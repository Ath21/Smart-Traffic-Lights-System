using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DetectionStore.Business;
using DetectionStore.Models.Requests;

namespace DetectionStore.Controllers;

// ============================================================
// Sensor Layer / Detection Service - Detection Events
// Scoped strictly to THIS intersection.
// ============================================================

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
    // GET: api/detections/local
    // Role: Anonymous
    // Description: Get active detection events for THIS intersection.
    // ============================================================
    [HttpGet("local")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLocalEvents()
    {
        var events = await _business.GetActiveEventsAsync();
        return Ok(events);
    }

    // ============================================================
    // POST: api/detections/event
    // Role: TrafficOperator, Admin
    // Description: Report a new detection event for THIS intersection.
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
