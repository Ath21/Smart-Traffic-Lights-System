using System;
using AutoMapper;
using TrafficAnalyticsData.Entities;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficAnalyticsStore.Publishers.Congestion;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficAnalyticsStore.Repository.Alerts;
using TrafficAnalyticsStore.Repository.Summary;
using TrafficMessages;

namespace TrafficAnalyticsStore.Business;

public class TrafficAnalyticsService : ITrafficAnalyticsService
{
    private readonly IDailySummaryRepository _summaryRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly IMapper _mapper;

    // Publishers
    private readonly ITrafficSummaryPublisher _summaryPublisher;
    private readonly ITrafficCongestionPublisher _congestionPublisher;
    private readonly ITrafficIncidentPublisher _incidentPublisher;

    public TrafficAnalyticsService(
        IDailySummaryRepository summaryRepo,
        IAlertRepository alertRepo,
        IMapper mapper,
        ITrafficSummaryPublisher summaryPublisher,
        ITrafficCongestionPublisher congestionPublisher,
        ITrafficIncidentPublisher incidentPublisher)
    {
        _summaryRepo = summaryRepo;
        _alertRepo = alertRepo;
        _mapper = mapper;
        _summaryPublisher = summaryPublisher;
        _congestionPublisher = congestionPublisher;
        _incidentPublisher = incidentPublisher;
    }

    // ----------------- Queries -----------------

    public async Task<CongestionDto?> GetCurrentCongestionAsync(Guid intersectionId)
    {
        var today = DateTime.UtcNow.Date;

        var summary = (await _summaryRepo.GetByIntersectionAsync(intersectionId, today)).FirstOrDefault();
        if (summary == null) return null;

        return new CongestionDto
        {
            IntersectionId = summary.IntersectionId,
            CongestionLevel = summary.CongestionLevel,
            VehicleCount = summary.VehicleCount,
            AvgSpeed = summary.AvgSpeed,
            Timestamp = summary.Date
        };
    }

    public async Task<IEnumerable<IncidentDto>> GetIncidentsAsync(Guid intersectionId)
    {
        var alerts = await _alertRepo.GetByIntersectionAsync(intersectionId);
        return _mapper.Map<IEnumerable<IncidentDto>>(alerts);
    }

    public async Task<SummaryDto?> GetDailySummaryAsync(Guid intersectionId, DateTime date)
    {
        var summary = (await _summaryRepo.GetByIntersectionAsync(intersectionId, date)).FirstOrDefault();
        return summary == null ? null : _mapper.Map<SummaryDto>(summary);
    }

    public async Task<IEnumerable<SummaryDto>> GetDailyReportsAsync()
    {
        var summaries = await _summaryRepo.GetAllAsync();
        var today = DateTime.UtcNow.Date;

        var todaySummaries = summaries.Where(s => s.Date.Date == today);
        return _mapper.Map<IEnumerable<SummaryDto>>(todaySummaries);
    }

    // ----------------- Commands -----------------

    public async Task AddOrUpdateSummaryAsync(SummaryDto dto)
    {
        var existing = (await _summaryRepo.GetByIntersectionAsync(dto.IntersectionId, dto.Date)).FirstOrDefault();

        if (existing == null)
        {
            var entity = _mapper.Map<DailySummary>(dto);
            entity.SummaryId = Guid.NewGuid();
            await _summaryRepo.AddAsync(entity);
            dto.SummaryId = entity.SummaryId;
        }
        else
        {
            existing.VehicleCount += dto.VehicleCount;
            existing.AvgSpeed = (existing.AvgSpeed + dto.AvgSpeed) / 2;
            existing.CongestionLevel = dto.CongestionLevel;
            await _summaryRepo.UpdateAsync(existing);

            dto.SummaryId = existing.SummaryId;
        }

        // Publish summary
        var summaryMessage = new TrafficSummaryMessage(
            dto.SummaryId,
            dto.IntersectionId,
            dto.Date,
            dto.AvgSpeed,
            dto.VehicleCount,
            dto.CongestionLevel
        );
        await _summaryPublisher.PublishSummaryAsync(summaryMessage);

        // Publish congestion if applicable
        if (dto.CongestionLevel is "High" or "Medium" or "Low")
        {
            var congestionMessage = new TrafficCongestionMessage(
                Guid.NewGuid(),
                dto.IntersectionId,
                dto.CongestionLevel,
                $"Congestion {dto.CongestionLevel} at intersection {dto.IntersectionId}",
                DateTime.UtcNow
            );
            await _congestionPublisher.PublishCongestionAsync(congestionMessage);
        }
    }

    public async Task ReportIncidentAsync(IncidentDto dto)
    {
        var entity = _mapper.Map<Alert>(dto);
        entity.AlertId = dto.AlertId != Guid.Empty ? dto.AlertId : Guid.NewGuid();
        entity.CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt;

        await _alertRepo.AddAsync(entity);

        // Publish incident
        var incidentMessage = new TrafficIncidentMessage(
            entity.AlertId,
            entity.IntersectionId,
            entity.Message,
            entity.CreatedAt
        );
        await _incidentPublisher.PublishIncidentAsync(incidentMessage);
    }
}