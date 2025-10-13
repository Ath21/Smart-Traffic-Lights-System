using System.Text;
using System.Globalization;
using LogStore.Models.Responses;
using LogData.Repositories.Search;
using LogData.Collections;
using LogData.Extensions;

namespace LogStore.Business;

public class LogBusiness : ILogBusiness
{
    private readonly ISearchLogRepository _searchRepo;
    private readonly ILogger<LogBusiness> _logger;

    public LogBusiness(
        ISearchLogRepository searchRepo,
        ILogger<LogBusiness> logger)
    {
        _searchRepo = searchRepo;
        _logger = logger;
    }

    // ============================================================
    // SEARCH METHOD
    // ============================================================
    public async Task<IEnumerable<SearchLogResponse>> SearchLogsAsync(
        string? layer,
        string? service,
        string? type,
        DateTime? from,
        DateTime? to)
    {
        _logger.LogInformation(
            "[BUSINESS] SearchLogsAsync called with layer={Layer}, service={Service}, type={Type}, from={From}, to={To}",
            layer, service, type, from, to);

        var results = await _searchRepo.SearchAsync(layer, service, type, from, to);
        _logger.LogInformation("[BUSINESS] Retrieved {Count} logs from repository", results.Count());

        var responses = new List<SearchLogResponse>();

        foreach (var obj in results)
        {
            switch (obj)
            {
                case AuditLogCollection audit:
                    responses.Add(MapAuditLog(audit));
                    break;

                case ErrorLogCollection error:
                    responses.Add(MapErrorLog(error));
                    break;

                case FailoverLogCollection failover:
                    responses.Add(MapFailoverLog(failover));
                    break;
            }
        }

        _logger.LogInformation("[BUSINESS] Mapped {Count} results to SearchLogResponse", responses.Count);
        return responses;
    }

    // ============================================================
    // EXPORT: CSV
    // ============================================================
    public async Task<byte[]> ExportLogsToCsvAsync(IEnumerable<object> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Layer,Service,Type,Message");

        foreach (var log in logs)
        {
            var timestamp = log.GetType().GetProperty("Timestamp")?.GetValue(log)?.ToString() ?? "";
            var layer = log.GetType().GetProperty("Layer")?.GetValue(log)?.ToString() ?? "";
            var service = log.GetType().GetProperty("Service")?.GetValue(log)?.ToString() ?? "";
            var type = log.GetType().Name.Replace("Collection", "");
            var message = log.GetType().GetProperty("Message")?.GetValue(log)?.ToString()?.Replace(",", " ") ?? "";

            sb.AppendLine($"{timestamp},{layer},{service},{type},\"{message}\"");
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // ============================================================
    // MANUAL MAPPERS
    // ============================================================

    private static SearchLogResponse MapAuditLog(AuditLogCollection log)
    {
        var response = new SearchLogResponse
        {
            LogType = "Audit",
            CorrelationId = log.CorrelationId,
            Timestamp = log.Timestamp,
            Layer = log.Layer ?? string.Empty,
            Service = log.Service ?? string.Empty,
            Action = log.Action,
            Message = log.Message,
            Metadata = BsonExtensions.ToDictionary(log.Metadata)
        };

        return response;
    }

    private static SearchLogResponse MapErrorLog(ErrorLogCollection log)
    {
        var response = new SearchLogResponse
        {
            LogType = "Error",
            CorrelationId = log.CorrelationId,
            Timestamp = log.Timestamp,
            Layer = log.Layer ?? string.Empty,
            Service = log.Service ?? string.Empty,
            Action = log.Action,
            Message = log.Message,
            Metadata = BsonExtensions.ToDictionary(log.Metadata)
        };

        return response;
    }

    private static SearchLogResponse MapFailoverLog(FailoverLogCollection log)
    {
        var response = new SearchLogResponse
        {
            LogType = "Failover",
            CorrelationId = log.CorrelationId,
            Timestamp = log.Timestamp,
            Layer = log.Layer ?? string.Empty,
            Service = log.Service ?? string.Empty,
            Action = log.Action,
            Message = log.Message,
            Metadata = BsonExtensions.ToDictionary(log.Metadata)
        };

        return response;
    }
}
