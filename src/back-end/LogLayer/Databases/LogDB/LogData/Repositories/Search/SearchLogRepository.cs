using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogData.Collections;
using MongoDB.Driver;

namespace LogData.Repositories.Search;

public class SearchLogRepository : ISearchLogRepository
{
    private readonly LogDbContext _context;

    public SearchLogRepository(LogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BaseLogCollection>> SearchAsync(
        string? layer,
        string? service,
        string? type,
        DateTime? from,
        DateTime? to)
    {
        // Build reusable filters
        var auditFilter = BuildFilter(_context.AuditLogs, layer, service, from, to);
        var errorFilter = BuildFilter(_context.ErrorLogs, layer, service, from, to);
        var failoverFilter = BuildFilter(_context.FailoverLogs, layer, service, from, to);

        var results = new List<BaseLogCollection>();

        // Apply optional type filtering
        if (string.IsNullOrEmpty(type) || type.Equals("audit", StringComparison.OrdinalIgnoreCase))
        {
            results.AddRange(await _context.AuditLogs.Find(auditFilter).ToListAsync());
        }

        if (string.IsNullOrEmpty(type) || type.Equals("error", StringComparison.OrdinalIgnoreCase))
        {
            results.AddRange(await _context.ErrorLogs.Find(errorFilter).ToListAsync());
        }

        if (string.IsNullOrEmpty(type) || type.Equals("failover", StringComparison.OrdinalIgnoreCase))
        {
            results.AddRange(await _context.FailoverLogs.Find(failoverFilter).ToListAsync());
        }

        // Unified ordered response (descending by timestamp)
        return results.OrderByDescending(x => x.Timestamp);
    }

    private static FilterDefinition<T> BuildFilter<T>(
        IMongoCollection<T> collection,
        string? layer,
        string? service,
        DateTime? from,
        DateTime? to)
        where T : BaseLogCollection
    {
        var builder = Builders<T>.Filter;
        var filters = new List<FilterDefinition<T>>();

        if (!string.IsNullOrWhiteSpace(layer))
            filters.Add(builder.Eq(x => x.Layer, layer));

        if (!string.IsNullOrWhiteSpace(service))
            filters.Add(builder.Eq(x => x.Service, service));

        if (from.HasValue)
            filters.Add(builder.Gte(x => x.Timestamp, from.Value));

        if (to.HasValue)
            filters.Add(builder.Lte(x => x.Timestamp, to.Value));

        return filters.Count > 0 ? builder.And(filters) : builder.Empty;
    }
}
