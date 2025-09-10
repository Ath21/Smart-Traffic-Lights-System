using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficLightData;

public class TrafficLightDbContextFactory : IDesignTimeDbContextFactory<TrafficLightDbContext>
{
    public TrafficLightDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TrafficLightDbContext>()
            .UseNpgsql(
                "Host=traffic_light_db;Port=5432;Database=TrafficLightDb;Username=postgres;Password=postgres123;Include Error Detail=true"
            )
            .Options;

        return new TrafficLightDbContext(options, new ConfigurationBuilder().Build());
    }
}
