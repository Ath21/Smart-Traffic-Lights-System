using System;
using DetectionData.Collection.Count;

namespace DetectionData.Repositories.Cyclist;

public interface ICyclistCountRepository
{
    Task AddAsync(CyclistCount record);
    Task<List<CyclistCount>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}