using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrafficLightCoordinatorData;

public class TrafficLightCoordinatorDbContextFactory : IDesignTimeDbContextFactory<TrafficLightCoordinatorDbContext>
{
    public TrafficLightCoordinatorDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TrafficLightCoordinatorDbContext>()
            .UseNpgsql("Host=localhost;Database=TrafficLightDb;Username=traffic;Password=traffic")
            .Options;

        return new TrafficLightCoordinatorDbContext(options);
    }
}
