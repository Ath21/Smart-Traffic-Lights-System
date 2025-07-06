
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsData;

public class TrafficDataAnalyticsDbContext : DbContext
{
    public TrafficDataAnalyticsDbContext(DbContextOptions<TrafficDataAnalyticsDbContext> options)
        : base(options) { }

    public DbSet<VehicleCount> VehicleCounts { get; set; }
    public DbSet<PedestrianCount> PedestrianCounts { get; set; }
    public DbSet<CyclistCount> CyclistCounts { get; set; }
    public DbSet<DailySummary> DailySummaries { get; set; }
    public DbSet<CongestionAlert> CongestionAlerts { get; set; }
    public DbSet<Intersection> Intersections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleCount>().ToTable("vehicle_counts");
        modelBuilder.Entity<PedestrianCount>().ToTable("pedestrian_counts");
        modelBuilder.Entity<CyclistCount>().ToTable("cyclist_counts");
        modelBuilder.Entity<DailySummary>().ToTable("daily_summaries");
        modelBuilder.Entity<CongestionAlert>().ToTable("congestion_alerts");
        modelBuilder.Entity<Intersection>().ToTable("intersections");
    }
}