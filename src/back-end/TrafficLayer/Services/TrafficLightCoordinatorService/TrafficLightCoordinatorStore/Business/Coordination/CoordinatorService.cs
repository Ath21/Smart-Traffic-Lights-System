using System;
using System.Text.Json;
using AutoMapper;
using TrafficLightCacheData.Repositories.Config;
using TrafficLightCacheData.Repositories.Intersect;
using TrafficLightCoordinatorStore.Models.Dtos;
using TrafficLightCoordinatorStore.Publishers.Logs;
using TrafficLightCoordinatorStore.Publishers.Update;

using TrafficMessages;

namespace TrafficLightCoordinatorStore.Business.Coordination;

public class CoordinatorService : ICoordinatorService
{
    private readonly IIntersectionRepository _intersections;
    private readonly ITrafficConfigurationRepository _configs;
    private readonly ILightUpdatePublisher _lightPublisher;
    private readonly ITrafficLogPublisher _logPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<CoordinatorService> _logger;

    private const string ServiceTag = "[" + nameof(CoordinatorService) + "]";

    public CoordinatorService(
        IIntersectionRepository intersections,
        ITrafficConfigurationRepository configs,
        ILightUpdatePublisher lightPublisher,
        ITrafficLogPublisher logPublisher,
        IMapper mapper,
        ILogger<CoordinatorService> logger)
    {
        _intersections = intersections;
        _configs = configs;
        _lightPublisher = lightPublisher;
        _logPublisher = logPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ConfigDto> UpsertConfigAsync(Guid intersectionId, string patternJson, CancellationToken ct)
    {
        using var jsonDoc = JsonDocument.Parse(patternJson);

        var entity = await _configs.AddAsync(
            intersectionId,
            jsonDoc,
            DateTimeOffset.UtcNow,
            reason: "API update",
            changeRef: Guid.NewGuid().ToString("N"),
            createdBy: "CoordinatorAPI",
            ct);

        var dto = _mapper.Map<ConfigDto>(entity);

        _logger.LogInformation("{Tag} Stored config {ConfigId} for intersection {IntersectionId}",
            ServiceTag, dto.ConfigId, dto.IntersectionId);

        await _logPublisher.PublishAuditAsync(
            action: "CONFIG_UPSERT",
            details: $"Pattern updated for intersection {intersectionId}",
            metadata: dto,
            ct);

        return dto;
    }


    public async Task<ConfigDto?> GetConfigAsync(Guid intersectionId, CancellationToken ct)
    {
        var entity = await _configs.GetLatestAsync(intersectionId, ct);
        return entity is null ? null : _mapper.Map<ConfigDto>(entity);
    }

    public async Task<PriorityDto> HandlePriorityAsync(PriorityDto dto, CancellationToken ct)
    {
        // Construct and publish update
        var updateMessage = new TrafficLightUpdateMessage(
            IntersectionId: dto.IntersectionId,
            LightId: Guid.NewGuid(), // TODO: resolve from DB if needed
            CurrentState: dto.AppliedPattern,
            UpdatedAt: dto.AppliedAt.UtcDateTime
        );

        await _lightPublisher.PublishAsync(updateMessage, ct);

        await _logPublisher.PublishAuditAsync(
            action: "PRIORITY_APPLIED",
            details: $"Priority {dto.PriorityType} applied",
            metadata: dto,
            ct);

        _logger.LogInformation("{Tag} Applied priority {Type} -> {Pattern}", ServiceTag, dto.PriorityType, dto.AppliedPattern);

        return dto;
    }

    public async Task<CongestionDto> HandleCongestionAsync(CongestionDto dto, CancellationToken ct)
    {
        var updateMessage = new TrafficLightUpdateMessage(
            IntersectionId: dto.IntersectionId,
            LightId: Guid.NewGuid(),
            CurrentState: dto.AppliedPattern,
            UpdatedAt: dto.AppliedAt.UtcDateTime
        );

        await _lightPublisher.PublishAsync(updateMessage, ct);

        await _logPublisher.PublishAuditAsync(
            action: "CONGESTION_APPLIED",
            details: $"Congestion strategy applied",
            metadata: dto,
            ct);

        _logger.LogInformation("{Tag} Applied congestion strategy -> {Pattern}", ServiceTag, dto.AppliedPattern);

        return dto;
    }
}