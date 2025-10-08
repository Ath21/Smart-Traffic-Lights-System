using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TrafficLightData.Entities;
using TrafficLightData.Settings;

namespace TrafficLightData;

public class TrafficLightDbContext : DbContext
{
    private readonly TrafficLightDbSettings _settings;

    public TrafficLightDbContext(DbContextOptions<TrafficLightDbContext> options, IOptions<TrafficLightDbSettings> settings)
        : base(options)
    {
        _settings = settings.Value;
    }

    public DbSet<IntersectionEntity> Intersections { get; set; }
    public DbSet<TrafficLightEntity> TrafficLights { get; set; }
    public DbSet<TrafficConfigurationEntity> TrafficConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relationships
        modelBuilder.Entity<IntersectionEntity>()
            .HasMany(i => i.TrafficLights)
            .WithOne(t => t.Intersection)
            .HasForeignKey(t => t.IntersectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IntersectionEntity>()
            .HasMany(i => i.Configurations)
            .WithOne(c => c.Intersection)
            .HasForeignKey(c => c.IntersectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision for MSSQL
        modelBuilder.Entity<IntersectionEntity>()
            .Property(i => i.Latitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<IntersectionEntity>()
            .Property(i => i.Longitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<TrafficLightEntity>()
            .Property(l => l.Latitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<TrafficLightEntity>()
            .Property(l => l.Longitude).HasColumnType("decimal(10,7)");

        // Default timestamps
        modelBuilder.Entity<IntersectionEntity>()
            .Property(i => i.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<TrafficConfigurationEntity>()
            .Property(c => c.LastUpdated)
            .HasDefaultValueSql("GETUTCDATE()");

        base.OnModelCreating(modelBuilder);
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