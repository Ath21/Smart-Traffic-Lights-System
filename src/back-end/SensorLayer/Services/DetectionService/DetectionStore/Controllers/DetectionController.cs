using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DetectionStore.Business;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Controllers;

[ApiController]
[Route("api/detections")]
public class DetectionController : ControllerBase
{
    private readonly IDetectionBusiness _business;
    private readonly IMapper _mapper;

    public DetectionController(IDetectionBusiness business, IMapper mapper)
    {
        _business = business;
        _mapper = mapper;
    }

    [HttpGet("{intersectionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSnapshot(Guid intersectionId)
    {
        var dto = await _business.GetSnapshotAsync(intersectionId);
        if (dto == null) return NotFound();

        return Ok(_mapper.Map<DetectionSnapshotResponse>(dto));
    }

    [HttpGet("{intersectionId:guid}/history")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> GetHistory(Guid intersectionId)
    {
        var dtos = await _business.GetHistoryAsync(intersectionId);
        return Ok(_mapper.Map<IEnumerable<DetectionHistoryResponse>>(dtos));
    }

    [HttpPost("emergency")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> RecordEmergency([FromBody] RecordEmergencyVehicleRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.EmergencyVehicleDto>(request);
        var result = await _business.RecordEmergencyAsync(dto);

        return Ok(_mapper.Map<EmergencyVehicleResponse>(result));
    }

    [HttpPost("public-transport")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> RecordPublicTransport([FromBody] RecordPublicTransportRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.PublicTransportDto>(request);
        var result = await _business.RecordPublicTransportAsync(dto);

        return Ok(_mapper.Map<PublicTransportResponse>(result));
    }

    [HttpPost("incident")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> RecordIncident([FromBody] RecordIncidentRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.IncidentDto>(request);
        var result = await _business.RecordIncidentAsync(dto);

        return Ok(_mapper.Map<IncidentResponse>(result));
    }
}
