using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorData;

public sealed class TrafficLightCoordinatorDbContext : DbContext
{
    public DbSet<Intersection> Intersections => Set<Intersection>();
    public DbSet<TrafficLight> TrafficLights => Set<TrafficLight>();
    public DbSet<TrafficConfiguration> TrafficConfigurations => Set<TrafficConfiguration>();

    public TrafficLightCoordinatorDbContext(DbContextOptions<TrafficLightCoordinatorDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Use a dedicated schema
        b.HasDefaultSchema("traffic");

        // Ensure PostGIS is enabled (migration will run CREATE EXTENSION)
        b.HasPostgresExtension("postgis");

        // INTERSECTIONS
        b.Entity<Intersection>(e =>
        {
            e.ToTable("intersections");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("intersection_id");
            e.Property(x => x.Name).HasMaxLength(180).IsRequired();
            e.Property(x => x.Description).HasColumnType("text");
            e.Property(x => x.InstalledAt).HasColumnName("installed_at");
            e.Property(x => x.Status).HasMaxLength(40).IsRequired();

            // GEOGRAPHY(Point,4326) via NetTopologySuite
            e.Property(x => x.Location)
                .HasColumnName("location")
                .HasColumnType("geography (point,4326)");

            e.HasIndex(x => x.Status);
        });

        // TRAFFIC_LIGHTS
        b.Entity<TrafficLight>(e =>
        {
            e.ToTable("traffic_lights");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("light_id");
            e.Property(x => x.IntersectionId).IsRequired();
            e.Property(x => x.CurrentState).HasMaxLength(64).IsRequired()
                .HasColumnName("current_state");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(x => new { x.IntersectionId, x.UpdatedAt });

            e.HasOne(x => x.Intersection)
                .WithMany(i => i.TrafficLights)
                .HasForeignKey(x => x.IntersectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TRAFFIC_CONFIGURATIONS
        b.Entity<TrafficConfiguration>(e =>
        {
            e.ToTable("traffic_configurations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("config_id");
            e.Property(x => x.IntersectionId).IsRequired();

            // Map System.Text.Json.JsonDocument to jsonb
            e.Property(x => x.Pattern)
                .HasColumnName("pattern")
                .HasColumnType("jsonb")
                .IsRequired();

            e.Property(x => x.EffectiveFrom).HasColumnName("effective_from");

            e.HasIndex(x => new { x.IntersectionId, x.EffectiveFrom });

            e.HasOne(x => x.Intersection)
                .WithMany(i => i.Configurations)
                .HasForeignKey(x => x.IntersectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}