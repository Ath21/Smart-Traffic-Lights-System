using System;
using DetectionData;
using DetectionData.Collections.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Pedestrian;

public class PedestrianCountRepository : Repository<PedestrianCount>, IPedestrianCountRepository
{
    public PedestrianCountRepository(DetectionDbContext context)
        : base(context.PedestrianCounts) { }
}
