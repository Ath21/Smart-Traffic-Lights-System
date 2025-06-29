using System;
using MongoDB.Driver;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Collections;

namespace TrafficDataAnalyticsStore.Repository;

public class MongoDbWriter : IMongoDbWriter
{
    private readonly TrafficDataAnalyticsDbContext _context;

    public MongoDbWriter(TrafficDataAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetAllIntersectionIdsAsync()
    {
        var ids = await _context.IntersectionsCollection
            .Find(_ => true)
            .Project(i => i.IntersectionId)
            .ToListAsync();

        return ids;
    }

    public async Task InsertDailySummaryAsync(DailySummary summary)
    {
        await _context.DailySummariesCollection.InsertOneAsync(summary);
    }
}
