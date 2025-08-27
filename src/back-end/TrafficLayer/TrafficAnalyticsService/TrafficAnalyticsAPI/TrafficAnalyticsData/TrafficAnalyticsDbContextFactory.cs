using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficAnalyticsData;

public class TrafficAnalyticsDbContextFactory : IDesignTimeDbContextFactory<TrafficAnalyticsDbContext>
{
    public TrafficAnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrafficAnalyticsDbContext>();

        // For design-time (migrations)
        // Replace with your actual PostgreSQL connection string
        optionsBuilder.UseNpgsql("Host=traffic_analytics_postgres;Port=5432;Database=TrafficDataDb;Username=postgres;Password=postgres123!@#");

        return new TrafficAnalyticsDbContext(optionsBuilder.Options, new ConfigurationBuilder().Build());
    }
}
