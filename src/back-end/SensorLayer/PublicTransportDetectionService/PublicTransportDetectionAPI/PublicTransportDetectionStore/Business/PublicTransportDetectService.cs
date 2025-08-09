using System;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using PublicTransportDetectionStore.Models;
using PublicTransportDetectionStore.Repositories;

namespace PublicTransportDetectionStore.Business;

public class PublicTransportDetectService : IPublicTransportDetectService
{
    private readonly IPublicTransportDetectionRepository _repository;
    private readonly IMapper _mapper;

    public PublicTransportDetectService(
        IPublicTransportDetectionRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

        /// <summary>
        /// Gets public transport detections from the repository
        /// </summary>
        public async Task<List<PublicTransportDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId = null,
            string? routeId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null)
        {
            var detections = await _repository.QueryAsync(
                intersectionId,
                routeId,         // ✅ Pass routeId explicitly
                startTime,
                endTime,
                limit
            );

            return _mapper.Map<List<PublicTransportDetectionReadDto>>(detections);
        }

        /// <summary>
        /// Inserts a new public transport detection
        /// </summary>
        public async Task<PublicTransportDetectionResponseDto> AddDetectionAsync(PublicTransportDetectionCreateDto createDto)
        {
            var entity = _mapper.Map<PublicTransportDetection>(createDto);
            var detectionId = await _repository.InsertAsync(entity);

            return new PublicTransportDetectionResponseDto
            {
                DetectionId = detectionId,
                Status = "Success"
            };
        }

}
