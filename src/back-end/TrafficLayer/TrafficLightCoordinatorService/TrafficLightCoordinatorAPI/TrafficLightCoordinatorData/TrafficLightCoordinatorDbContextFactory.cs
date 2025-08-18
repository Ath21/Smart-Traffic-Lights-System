using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrafficLightCoordinatorData;

public class TrafficLightCoordinatorDbContextFactory
    : IDesignTimeDbContextFactory<TrafficLightCoordinatorDbContext>
{
    public TrafficLightCoordinatorDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TrafficLightCoordinatorDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=traffic_lights",
                npgsql =>
                {
                    // enable NetTopologySuite for PostGIS
                    npgsql.UseNetTopologySuite();
                })
            .Options;

        return new TrafficLightCoordinatorDbContext(options);
    }
}
