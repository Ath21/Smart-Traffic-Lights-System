using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IncidentDetectionStore.Models;

namespace IncidentDetectionStore.Business
{
    public interface IIncidentDetectService
    {
        Task<IncidentDetectionResponseDto> AddDetectionAsync(IncidentDetectionCreateDto dto);
        Task<IEnumerable<IncidentDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null);
    }
}
