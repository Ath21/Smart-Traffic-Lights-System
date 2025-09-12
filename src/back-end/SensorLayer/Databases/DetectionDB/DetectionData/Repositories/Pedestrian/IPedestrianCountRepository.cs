using System;
using DetectionData.Collections.Count;

namespace DetectionData.Repositories.Pedestrian;

public interface IPedestrianCountRepository
{
    Task AddAsync(PedestrianCount record);
    Task<List<PedestrianCount>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}