using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficLightData;

public class TrafficLightDbContextFactory : IDesignTimeDbContextFactory<TrafficLightDbContext>
{
    public TrafficLightDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TrafficLightDbContext>()
            .UseSqlServer(
                "Server=traffic_light_db,1433;Database=TrafficLightDB;User Id=sa;Password=MyPass@word;TrustServerCertificate=True"
            )
            .Options;

        return new TrafficLightDbContext(options, new ConfigurationBuilder().Build());
    }
}
