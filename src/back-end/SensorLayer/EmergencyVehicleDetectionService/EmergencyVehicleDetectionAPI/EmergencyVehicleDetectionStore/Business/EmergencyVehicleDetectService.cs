using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using EmergencyVehicleDetectionStore.Models;
using EmergencyVehicleDetectionStore.Repositories;

namespace EmergencyVehicleDetectionStore.Business;

public class EmergencyVehicleDetectService : IEmergencyVehicleDetectService
{
    private readonly IEmergencyVehicleDetectionRepository _repository;
    private readonly IMapper _mapper;

    public EmergencyVehicleDetectService(IEmergencyVehicleDetectionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EmergencyVehicleDetectionReadDto> AddDetectionAsync(EmergencyVehicleDetectionCreateDto dto)
    {
        var entity = _mapper.Map<EmergencyVehicleDetection>(dto);
        await _repository.InsertAsync(entity);
        return _mapper.Map<EmergencyVehicleDetectionReadDto>(entity);
    }

    public async Task<List<EmergencyVehicleDetectionReadDto>> GetDetectionsAsync(Guid? intersectionId, DateTime? start, DateTime? end, int? limit)
    {
        var detections = await _repository.QueryAsync(intersectionId, start, end, limit);
        return _mapper.Map<List<EmergencyVehicleDetectionReadDto>>(detections);
    }
}