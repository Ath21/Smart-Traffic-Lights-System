using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using VehicleDetectionStore.Models;
using VehicleDetectionStore.Repositories;

namespace VehicleDetectionStore.Business;

public class VehicleDetectService : IVehicleDetectService
{
    private readonly IVehicleDetectionRepository _repository;
    private readonly IMapper _mapper;

    public VehicleDetectService(IVehicleDetectionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<VehicleDetectionReadDto> AddDetectionAsync(VehicleDetectionCreateDto dto)
    {
        var entity = _mapper.Map<VehicleDetection>(dto);
        await _repository.InsertAsync(entity);
        return _mapper.Map<VehicleDetectionReadDto>(entity);
    }

    public async Task<List<VehicleDetectionReadDto>> GetDetectionsAsync(Guid? intersectionId, DateTime? start, DateTime? end, int? limit)
    {
        var detections = await _repository.QueryAsync(intersectionId, start, end, limit);
        return _mapper.Map<List<VehicleDetectionReadDto>>(detections);
    }
}