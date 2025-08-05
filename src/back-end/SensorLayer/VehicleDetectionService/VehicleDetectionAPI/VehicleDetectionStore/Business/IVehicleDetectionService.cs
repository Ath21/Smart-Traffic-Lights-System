using System;
using VehicleDetectionStore.Models;

namespace VehicleDetectionStore.Business;

public interface IVehicleDetectionService
{
    public Task<VehicleDetectionReadDto> AddDetectionAsync(VehicleDetectionCreateDto dto);
    public Task<List<VehicleDetectionReadDto>> GetDetectionsAsync(Guid? intersectionId, DateTime? start, DateTime? end, int? limit);
}
