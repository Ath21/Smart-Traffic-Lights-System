using System;
using AutoMapper;
using MongoDB.Driver;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public class SummaryService : ISummaryService
{
    private readonly TrafficDataAnalyticsDbContext _context;
    private readonly IMapper _mapper;

    public SummaryService(
        TrafficDataAnalyticsDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<DailySummaryDto>> GetLatestSummariesAsync()
    {
        var today = DateTime.UtcNow.Date;
        var summaries = await _context.DailySummariesCollection
            .Find(s => s.Date == today)
            .ToListAsync();

        return _mapper.Map<List<DailySummaryDto>>(summaries);
    }
}
