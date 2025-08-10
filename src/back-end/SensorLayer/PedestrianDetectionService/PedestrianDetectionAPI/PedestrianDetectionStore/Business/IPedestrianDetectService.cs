using PedestrianDetectionStore.Models;

namespace PedestrianDetectionStore.Business
{
    public interface IPedestrianDetectService
    {
        Task<PedestrianDetectionResponseDto> AddDetectionAsync(PedestrianDetectionCreateDto createDto);

        Task<List<PedestrianDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId,
            DateTime? startTime,
            DateTime? endTime,
            int? limit);
    }
}
