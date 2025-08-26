using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsData;

public class TrafficDataAnalyticsDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public TrafficDataAnalyticsDbContext(DbContextOptions<TrafficDataAnalyticsDbContext> options, IConfiguration configuration)
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
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
