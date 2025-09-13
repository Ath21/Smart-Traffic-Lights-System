using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData;

public class TrafficAnalyticsDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public TrafficAnalyticsDbContext(DbContextOptions<TrafficAnalyticsDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<DailySummary> DailySummaries { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // daily_summaries
        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.HasKey(e => e.SummaryId);
            entity.Property(e => e.CongestionLevel)
                  .HasMaxLength(50);
        });

        // alerts
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.AlertId);
            entity.Property(e => e.Type)
                  .HasMaxLength(50);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("PostgreSQLConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

}
