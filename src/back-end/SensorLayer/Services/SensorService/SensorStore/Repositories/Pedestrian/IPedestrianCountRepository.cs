using System;
using DetectionData.Collection.Count;

namespace SensorStore.Repositories.Pedestrian;

public interface IPedestrianCountRepository
{
    Task AddAsync(PedestrianCount record);
    Task<List<PedestrianCount>> GetHistoryAsync(Guid intersectionId, int limit = 50);
}