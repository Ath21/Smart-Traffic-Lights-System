using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficAnalyticsData;

public class TrafficAnalyticsDbContextFactory : IDesignTimeDbContextFactory<TrafficAnalyticsDbContext>
{
    public TrafficAnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrafficAnalyticsDbContext>();

        optionsBuilder.UseNpgsql("Host=traffic_analytics_db;Port=5432;Database=TrafficAnalyticsDB;Username=postgres;Password=postgres123");

        return new TrafficAnalyticsDbContext(optionsBuilder.Options, new ConfigurationBuilder().Build());
    }
}
