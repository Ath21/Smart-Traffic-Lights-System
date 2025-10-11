using System.Text;
using LogStore.Models.Responses;
using ReportLab = iText.Kernel.Pdf; // placeholder comment if using actual PDF generator
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using LogData.Repositories.Search;
using AutoMapper;
using LogData.Collections;


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

    // ============================================================
    // EXPORT: CSV
    // ============================================================
    public async Task<byte[]> ExportLogsToCsvAsync(IEnumerable<object> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Layer,Service,Type,Message");

        foreach (var log in logs)
        {
            // Using reflection for polymorphic types (Audit/Error/Failover)
            var timestamp = log.GetType().GetProperty("Timestamp")?.GetValue(log)?.ToString() ?? "";
            var layer = log.GetType().GetProperty("Layer")?.GetValue(log)?.ToString() ?? "";
            var service = log.GetType().GetProperty("Service")?.GetValue(log)?.ToString() ?? "";
            var type = log.GetType().GetProperty("LogType")?.GetValue(log)?.ToString()
                       ?? log.GetType().Name.Replace("Collection", "");
            var message = log.GetType().GetProperty("Message")?.GetValue(log)?.ToString()?.Replace(",", " ") ?? "";

            sb.AppendLine($"{timestamp},{layer},{service},{type},\"{message}\"");
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    // ============================================================
    // EXPORT: PDF
    // ============================================================
    public async Task<byte[]> ExportLogsToPdfAsync(IEnumerable<object> logs)
    {
        using var ms = new MemoryStream();

        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            var doc = new Document(pdf);
            doc.Add(new Paragraph("Log Export Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16));

            doc.Add(new Paragraph($"Generated at: {DateTime.UtcNow:u}")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(10));

            var table = new Table(5).UseAllAvailableWidth();
            table.AddHeaderCell("Timestamp");
            table.AddHeaderCell("Layer");
            table.AddHeaderCell("Service");
            table.AddHeaderCell("Type");
            table.AddHeaderCell("Message");

            foreach (var log in logs)
            {
                table.AddCell(log.GetType().GetProperty("Timestamp")?.GetValue(log)?.ToString() ?? "");
                table.AddCell(log.GetType().GetProperty("Layer")?.GetValue(log)?.ToString() ?? "");
                table.AddCell(log.GetType().GetProperty("Service")?.GetValue(log)?.ToString() ?? "");
                table.AddCell(log.GetType().GetProperty("LogType")?.GetValue(log)?.ToString()
                              ?? log.GetType().Name.Replace("Collection", ""));
                table.AddCell(log.GetType().GetProperty("Message")?.GetValue(log)?.ToString() ?? "");
            }

            doc.Add(table);
            doc.Close();
        }

        return await Task.FromResult(ms.ToArray());
    }
}