using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficLightCoordinatorData;

public class TrafficLightCoordinatorDbContextFactory : IDesignTimeDbContextFactory<TrafficLightCoordinatorDbContext>
{
    public TrafficLightCoordinatorDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TrafficLightCoordinatorDbContext>()
            .UseNpgsql(
                "Host=traffic_light_coordinator_postgres;Port=5432;Database=TrafficLightDb;Username=postgres;Password=postgres123;Include Error Detail=true"
            )
            .Options;

        return new TrafficLightCoordinatorDbContext(options, new ConfigurationBuilder().Build());
    }
}
