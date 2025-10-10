using System;

namespace LogData.Repositories.Search;

public interface ISearchLogRepository
{
    Task<IEnumerable<object>> SearchAsync(string? layer, string? service, string? type, DateTime? from, DateTime? to);
}
