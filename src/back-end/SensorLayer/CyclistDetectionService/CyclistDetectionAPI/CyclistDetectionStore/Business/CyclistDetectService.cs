using AutoMapper;
using CyclistDetectionStore.Models;
using CyclistDetectionStore.Repositories;
using DetectionData.TimeSeriesObjects;

namespace CyclistDetectionStore.Business
{
    public class CyclistDetectService : ICyclistDetectService
    {
        private readonly ICyclistDetectionRepository _repository;
        private readonly IMapper _mapper;

        public CyclistDetectService(ICyclistDetectionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CyclistDetectionResponseDto> CreateDetectionAsync(CyclistDetectionCreateDto createDto)
        {
            var entity = _mapper.Map<CyclistDetection>(createDto);
            var detectionId = await _repository.InsertAsync(entity);

            entity.DetectionId = detectionId;

            return _mapper.Map<CyclistDetectionResponseDto>(entity);
        }

        public async Task<List<CyclistDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId,
            DateTime? startTime,
            DateTime? endTime,
            int? limit)
        {
            var entities = await _repository.QueryAsync(intersectionId, startTime, endTime, limit);
            return _mapper.Map<List<CyclistDetectionReadDto>>(entities);
        }
    }
}
