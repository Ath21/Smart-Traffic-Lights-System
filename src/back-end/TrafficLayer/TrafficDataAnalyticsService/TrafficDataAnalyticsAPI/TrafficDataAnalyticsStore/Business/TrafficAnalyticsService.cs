using System;
using AutoMapper;
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
}
