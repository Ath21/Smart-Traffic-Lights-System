using System;
using AutoMapper;
using TrafficDataAnalyticsData.Entities;
using TrafficDataAnalyticsStore.Models.Dtos;
using TrafficDataAnalyticsStore.Repository.Alerts;
using TrafficDataAnalyticsStore.Repository.Summary;

namespace TrafficDataAnalyticsStore.Business;

public class TrafficAnalyticsService : ITrafficAnalyticsService
{
    private readonly IDailySummaryRepository _summaryRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly IMapper _mapper;

    public TrafficAnalyticsService(
        IDailySummaryRepository summaryRepo,
        IAlertRepository alertRepo,
        IMapper mapper)
    {
        _summaryRepo = summaryRepo;
        _alertRepo = alertRepo;
        _mapper = mapper;
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
        }
        else
        {
            existing.VehicleCount += dto.VehicleCount; // aggregate counts
            existing.AvgSpeed = (existing.AvgSpeed + dto.AvgSpeed) / 2; // simple avg merge
            existing.CongestionLevel = dto.CongestionLevel; // update latest level

            await _summaryRepo.UpdateAsync(existing);
        }
    }

    public async Task ReportIncidentAsync(IncidentDto dto)
    {
        var entity = _mapper.Map<Alert>(dto);
        entity.AlertId = dto.AlertId != Guid.Empty ? dto.AlertId : Guid.NewGuid();
        entity.CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt;

        await _alertRepo.AddAsync(entity);
    }
}
