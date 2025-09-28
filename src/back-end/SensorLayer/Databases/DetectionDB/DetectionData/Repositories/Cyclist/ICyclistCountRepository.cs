using System;
using DetectionData.Collections.Count;

namespace DetectionData.Repositories.Cyclist;

public interface ICyclistCountRepository
{
    Task InsertAsync(CyclistCount entity);
    Task<List<CyclistCount>> GetByIntersectionAsync(int intersectionId);
}