using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyclistDetectionStore.Models;

namespace CyclistDetectionStore.Business
{
    public interface ICyclistDetectService
    {
        Task<CyclistDetectionResponseDto> CreateDetectionAsync(CyclistDetectionCreateDto createDto);
        Task<List<CyclistDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null);
    }
}
