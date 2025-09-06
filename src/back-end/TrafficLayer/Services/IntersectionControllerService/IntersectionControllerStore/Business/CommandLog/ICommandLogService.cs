using System;

namespace IntersectionControllerStore.Business.CommandLog;

public interface ICommandLogService
{
    Task LogCommandAsync(Guid intersectionId, object command);
    Task<List<T>> GetRecentCommandsAsync<T>(Guid intersectionId, int count = 20);
}