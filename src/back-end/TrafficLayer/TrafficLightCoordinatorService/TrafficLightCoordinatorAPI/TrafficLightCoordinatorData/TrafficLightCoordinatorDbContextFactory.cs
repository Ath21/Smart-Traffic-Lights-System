using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficLightCoordinatorData;

public class TrafficLightCoordinatorDbContextFactory : IDesignTimeDbContextFactory<TrafficLightCoordinatorDbContext>
{
    public TrafficLightCoordinatorDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TrafficLightCoordinatorDbContext>();
        options.UseNpgsql("Host=traffic_light_coordinator_postgres;Port=5432;Database=TrafficLightDb;Username=traffic;Password=traffic;Include Error Detail=true");

        return new TrafficLightCoordinatorDbContext(options.Options, new ConfigurationBuilder().Build());
    }
}
