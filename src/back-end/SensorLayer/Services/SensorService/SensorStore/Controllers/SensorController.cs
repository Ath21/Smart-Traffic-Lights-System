using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorStore.Business;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

namespace SensorStore.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorsController : ControllerBase
{
    private readonly ISensorBusiness _business;
    private readonly IMapper _mapper;

    public SensorsController(ISensorBusiness business, IMapper mapper)
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

        return Ok(_mapper.Map<SensorSnapshotResponse>(dto));
    }

    [HttpGet("{intersectionId:guid}/history")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> GetHistory(Guid intersectionId)
    {
        var dtos = await _business.GetHistoryAsync(intersectionId);
        return Ok(_mapper.Map<IEnumerable<SensorHistoryResponse>>(dtos));
    }

    [HttpPost("update")]
    [Authorize(Roles = "TrafficOperator,Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateSensorSnapshotRequest request)
    {
        var dto = _mapper.Map<Models.Dtos.SensorSnapshotDto>(request);
        var updated = await _business.UpdateSnapshotAsync(dto, request.AvgSpeed);

        return Ok(_mapper.Map<SensorSnapshotResponse>(updated));
    }
}
