using System;
using EmergencyVehicleDetectionStore.Models;

namespace EmergencyVehicleDetectionStore.Business;

public interface IEmergencyVehicleDetectService
{
    public Task<EmergencyVehicleDetectionReadDto> AddDetectionAsync(EmergencyVehicleDetectionCreateDto dto);
    public Task<List<EmergencyVehicleDetectionReadDto>> GetDetectionsAsync(Guid? intersectionId, DateTime? start, DateTime? end, int? limit);
}

