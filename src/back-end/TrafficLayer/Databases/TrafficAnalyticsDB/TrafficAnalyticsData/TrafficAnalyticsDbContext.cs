using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData;

public class TrafficAnalyticsDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public TrafficAnalyticsDbContext(
        DbContextOptions<TrafficAnalyticsDbContext> options,
        IConfiguration? configuration = null) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<DailySummary> DailySummaries => Set<DailySummary>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DailySummary
        modelBuilder.Entity<DailySummary>(e =>
        {
            e.HasKey(s => s.SummaryId);

            e.Property(s => s.SummaryId).ValueGeneratedOnAdd();

            e.HasIndex(s => new { s.IntersectionId, s.Date })
                .HasDatabaseName("ix_summary_intersection_date");

            e.Property(s => s.Date).HasColumnType("date"); // stored as DATE
        });

        // Alerts
        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(a => a.AlertId);

            e.Property(a => a.AlertId).ValueGeneratedOnAdd();

            e.HasIndex(a => new { a.IntersectionId, a.CreatedAt })
                .HasDatabaseName("ix_alerts_intersection_created");

            e.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration is not null)
        {
            var connectionString = _configuration.GetConnectionString("PostgresConnection");

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });
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
