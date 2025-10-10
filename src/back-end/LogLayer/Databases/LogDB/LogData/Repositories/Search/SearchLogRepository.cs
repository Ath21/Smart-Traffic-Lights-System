using System;
using LogData.Collections;
using LogData.Repositories.Audit;
using LogData.Repositories.Error;
using LogData.Repositories.Failover;
using MongoDB.Driver;

namespace LogData.Repositories.Search;

public class SearchLogRepository : ISearchLogRepository
{
    private readonly LogDbContext _context;

    public SearchLogRepository(LogDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<object>> SearchAsync(
        string? layer,
        string? service,
        string? type,
        DateTime? from,
        DateTime? to)
    {
        // Filters setup
        var builderAudit = Builders<AuditLogCollection>.Filter;
        var builderError = Builders<ErrorLogCollection>.Filter;
        var builderFailover = Builders<FailoverLogCollection>.Filter;

        var auditFilter = builderAudit.Empty;
        var errorFilter = builderError.Empty;
        var failoverFilter = builderFailover.Empty;

        if (!string.IsNullOrWhiteSpace(layer))
        {
            auditFilter &= builderAudit.Eq(x => x.Layer, layer);
            errorFilter &= builderError.Eq(x => x.Layer, layer);
            failoverFilter &= builderFailover.Eq(x => x.Layer, layer);
        }

        if (!string.IsNullOrWhiteSpace(service))
        {
            auditFilter &= builderAudit.Eq(x => x.Service, service);
            errorFilter &= builderError.Eq(x => x.Service, service);
            failoverFilter &= builderFailover.Eq(x => x.Service, service);
        }

        if (from.HasValue)
        {
            auditFilter &= builderAudit.Gte(x => x.Timestamp, from.Value);
            errorFilter &= builderError.Gte(x => x.Timestamp, from.Value);
            failoverFilter &= builderFailover.Gte(x => x.Timestamp, from.Value);
        }

        if (to.HasValue)
        {
            auditFilter &= builderAudit.Lte(x => x.Timestamp, to.Value);
            errorFilter &= builderError.Lte(x => x.Timestamp, to.Value);
            failoverFilter &= builderFailover.Lte(x => x.Timestamp, to.Value);
        }

        var results = new List<object>();

        // Apply optional type filtering
        if (string.IsNullOrEmpty(type) || type.Equals("audit", StringComparison.OrdinalIgnoreCase))
        {
            var audit = await _context.AuditLogs.Find(auditFilter).ToListAsync();
            results.AddRange(audit);
        }

        if (string.IsNullOrEmpty(type) || type.Equals("error", StringComparison.OrdinalIgnoreCase))
        {
            var error = await _context.ErrorLogs.Find(errorFilter).ToListAsync();
            results.AddRange(error);
        }

        if (string.IsNullOrEmpty(type) || type.Equals("failover", StringComparison.OrdinalIgnoreCase))
        {
            var failover = await _context.FailoverLogs.Find(failoverFilter).ToListAsync();
            results.AddRange(failover);
        }

        // Unified, ordered response (descending by Timestamp)
        return results.OrderByDescending(x => ((dynamic)x).Timestamp);
    }
}
