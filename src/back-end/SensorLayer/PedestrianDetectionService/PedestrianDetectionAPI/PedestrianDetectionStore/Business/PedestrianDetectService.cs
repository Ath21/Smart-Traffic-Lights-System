using AutoMapper;
using DetectionData.TimeSeriesObjects;
using PedestrianDetectionStore.Models;
using PedestrianDetectionStore.Repositories;

namespace PedestrianDetectionStore.Business
{
    public class PedestrianDetectService : IPedestrianDetectService
    {
        private readonly IPedestrianDetectionRepository _repository;
        private readonly IMapper _mapper;

        public PedestrianDetectService(IPedestrianDetectionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PedestrianDetectionResponseDto> AddDetectionAsync(PedestrianDetectionCreateDto createDto)
        {
            var entity = _mapper.Map<PedestrianDetection>(createDto);
            var detectionId = await _repository.InsertAsync(entity);

            entity.DetectionId = detectionId;

            return _mapper.Map<PedestrianDetectionResponseDto>(entity);
        }

        public async Task<List<PedestrianDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId,
            DateTime? startTime,
            DateTime? endTime,
            int? limit)
        {
            var entities = await _repository.QueryAsync(intersectionId, startTime, endTime, limit);
            return _mapper.Map<List<PedestrianDetectionReadDto>>(entities);
        }
    }
}
