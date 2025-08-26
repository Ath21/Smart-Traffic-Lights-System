using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrafficDataAnalyticsData;

public class TrafficDataAnalyticsDbContextFactory : IDesignTimeDbContextFactory<TrafficDataAnalyticsDbContext>
{
    public TrafficDataAnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrafficDataAnalyticsDbContext>();

        // For design-time (migrations)
        // Replace with your actual PostgreSQL connection string
        optionsBuilder.UseNpgsql("Host=traffic_analytics_postgres;Port=5432;Database=TrafficDataDb;Username=postgres;Password=postgres123!@#");

        return new TrafficDataAnalyticsDbContext(optionsBuilder.Options, new ConfigurationBuilder().Build());
    }
}
