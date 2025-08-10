using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DetectionData.TimeSeriesObjects;
using IncidentDetectionStore.Models;
using IncidentDetectionStore.Repositories;
using Microsoft.Extensions.Logging;

namespace IncidentDetectionStore.Business
{
    public class IncidentDetectService : IIncidentDetectService
    {
        private readonly IIncidentDetectionRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<IncidentDetectService> _logger;

        public IncidentDetectService(
            IIncidentDetectionRepository repository,
            IMapper mapper,
            ILogger<IncidentDetectService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IncidentDetectionResponseDto> AddDetectionAsync(IncidentDetectionCreateDto dto)
        {
            _logger.LogInformation("Adding new incident detection for intersection {IntersectionId}", dto.IntersectionId);

            var detection = _mapper.Map<IncidentDetection>(dto);
            var id = await _repository.InsertAsync(detection);

            _logger.LogInformation("Incident detection {DetectionId} created successfully", id);

            return new IncidentDetectionResponseDto
            {
                DetectionId = id,
                Status = "Created"
            };
        }

        public async Task<IEnumerable<IncidentDetectionReadDto>> GetDetectionsAsync(
            Guid? intersectionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int? limit = null)
        {
            _logger.LogInformation("Querying incident detections with filters: intersectionId={IntersectionId}, startTime={StartTime}, endTime={EndTime}, limit={Limit}",
                intersectionId, startTime, endTime, limit);

            var detections = await _repository.QueryAsync(intersectionId, startTime, endTime, limit);
            return _mapper.Map<IEnumerable<IncidentDetectionReadDto>>(detections);
        }
    }
}
