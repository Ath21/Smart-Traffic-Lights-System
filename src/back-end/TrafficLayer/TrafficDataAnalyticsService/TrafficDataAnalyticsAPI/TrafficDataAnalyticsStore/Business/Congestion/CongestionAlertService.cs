using System;
using AutoMapper;
using MongoDB.Driver;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.Congestion;

public class CongestionAlertService : ICongestionAlertService
{
    private readonly TrafficDataAnalyticsDbContext _context;
    private readonly IMapper _mapper;

    public CongestionAlertService(
        TrafficDataAnalyticsDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CongestionAlertDto>> GetActiveAlertsAsync()
    {
        var recentThreshold = DateTime.UtcNow.AddHours(-2);
        var alerts = await _context.CongestionAlertsCollection
            .Find(a => a.Timestamp >= recentThreshold)
            .ToListAsync();
        
        return _mapper.Map<List<CongestionAlertDto>>(alerts);
    }
}
