using AutoMapper;
using LogData.Collections;
using LogData.Repositories.Audit;
using LogData.Repositories.Error;
using MongoDB.Driver;
using LogData.Repositories.Failover;
using LogData.Repositories.Search;
using LogStore.Models.Responses;

namespace LogStore.Business;

public class LogBusiness : ILogBusiness
{
    private readonly ISearchLogRepository _searchRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<LogBusiness> _logger;

    public LogBusiness(
        ISearchLogRepository searchRepo,
        IMapper mapper,
        ILogger<LogBusiness> logger)
    {
        _searchRepo = searchRepo;
        _mapper = mapper;
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
        _logger.LogInformation("[BUSINESS] SearchLogsAsync called with layer={Layer}, service={Service}, type={Type}, from={From}, to={To}", layer, service, type, from, to);

        var results = await _searchRepo.SearchAsync(layer, service, type, from, to);

        _logger.LogInformation("[BUSINESS] SearchLogsAsync retrieved {Count} results from repository", results.Count());

        // Convert each Mongo entity to SearchLogResponse
        var mapped = results.Select(obj => obj switch
        {
            AuditLogCollection audit => _mapper.Map<SearchLogResponse>(audit),
            ErrorLogCollection error => _mapper.Map<SearchLogResponse>(error),
            FailoverLogCollection failover => _mapper.Map<SearchLogResponse>(failover),
            _ => null
        }).Where(x => x != null)!;

        _logger.LogInformation("[BUSINESS] SearchLogsAsync mapped results to {MappedCount} SearchLogResponse objects", mapped.Count());

        return mapped;
    }
}