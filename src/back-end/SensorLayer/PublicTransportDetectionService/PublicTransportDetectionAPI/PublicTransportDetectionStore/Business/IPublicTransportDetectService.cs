using System;
using PublicTransportDetectionStore.Models;

namespace PublicTransportDetectionStore.Business;

public interface IPublicTransportDetectService
{
    Task<PublicTransportDetectionResponseDto> AddDetectionAsync(
    PublicTransportDetectionCreateDto createDto);

    Task<List<PublicTransportDetectionReadDto>> GetDetectionsAsync(
        Guid? intersectionId,
        string? routeId,            // ✅ Added routeId here
        DateTime? startTime,
        DateTime? endTime,
        int? limit);
}
