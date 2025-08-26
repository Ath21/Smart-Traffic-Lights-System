using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrafficDataAnalyticsData;

public class TrafficDataDbContextFactory : IDesignTimeDbContextFactory<TrafficDataDbContext>
{
    public TrafficDataDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrafficDataDbContext>();

        // For design-time (migrations)
        // Replace with your actual PostgreSQL connection string
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TrafficDataDb;Username=postgres;Password=postgres");

        return new TrafficDataDbContext(optionsBuilder.Options);
    }
}
