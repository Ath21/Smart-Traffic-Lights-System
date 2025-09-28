using System;
using DetectionData.Collections.Count;

namespace DetectionData.Repositories.Pedestrian;

public interface IPedestrianCountRepository
{
    Task InsertAsync(PedestrianCount entity);
    Task<List<PedestrianCount>> GetByIntersectionAsync(int intersectionId);
}
