using System;
using DetectionData;
using DetectionData.Collections.Count;
using MongoDB.Driver;

namespace DetectionData.Repositories.Cyclist;

public class CyclistCountRepository : Repository<CyclistCount>, ICyclistCountRepository
{
    public CyclistCountRepository(DetectionDbContext context)
        : base(context.CyclistCounts) { }
}
