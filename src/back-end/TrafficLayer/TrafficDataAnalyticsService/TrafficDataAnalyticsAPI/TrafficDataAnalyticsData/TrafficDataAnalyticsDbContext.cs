using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsData;

public class TrafficDataDbContext : DbContext
{
    public TrafficDataDbContext(DbContextOptions<TrafficDataDbContext> options)
        : base(options) { }

    public DbSet<DailySummary> DailySummaries { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
}
