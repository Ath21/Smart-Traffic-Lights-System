using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace TrafficLightCoordinatorData.Entities;

public sealed class CoordinatorDbContext : DbContext
{
    public DbSet<Intersection> Intersections => Set<Intersection>();
    public DbSet<TrafficLight> TrafficLights => Set<TrafficLight>();
    public DbSet<TrafficConfiguration> TrafficConfigurations => Set<TrafficConfiguration>();

    public CoordinatorDbContext(DbContextOptions<CoordinatorDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ---- intersections
        b.Entity<Intersection>(e =>
        {
            e.ToTable("intersections");
            e.HasKey(x => x.IntersectionId);
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            e.Property(x => x.InstalledAt).HasColumnName("installed_at").HasColumnType("timestamp");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();

            // GEOGRAPHY(Point,4326)
            e.Property(x => x.Location)
             .HasColumnName("location")
             .HasColumnType("geography(Point,4326)");

            // If you prefer JSON instead of geography, replace the property in the entity with:
            // public JsonDocument? Location { get; set; }
            // and map here as:
            // e.Property<JsonDocument?>(x => x.Location).HasColumnName("location").HasColumnType("jsonb");

            // Relations
            e.HasMany(x => x.TrafficLights)
             .WithOne(x => x.Intersection!)
             .HasForeignKey(x => x.IntersectionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.TrafficConfigurations)
             .WithOne(x => x.Intersection!)
             .HasForeignKey(x => x.IntersectionId)
             .OnDelete(DeleteBehavior.Cascade);

            // Useful indexes
            e.HasIndex(x => x.Name).HasDatabaseName("ix_intersections_name");
        });

        // ---- traffic_lights
        b.Entity<TrafficLight>(static e =>
        {
            e.ToTable("traffic_lights");
            e.HasKey(x => x.LightId);
            e.Property(x => x.CurrentState).HasColumnName("current_state").HasMaxLength(32).IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            e.Property(x => x.IntersectionId).HasColumnName("intersection_id");
            e.HasIndex(x => new { x.IntersectionId, x.CurrentState }).HasDatabaseName("ix_lights_intersection_state");
        });

        // ---- traffic_configurations
        b.Entity<TrafficConfiguration>(e =>
        {
            e.ToTable("traffic_configurations");
            e.HasKey(x => x.ConfigId);
            e.Property(x => x.IntersectionId).HasColumnName("intersection_id");
            e.Property(x => x.Pattern).HasColumnName("pattern").HasColumnType("jsonb").IsRequired();
            e.Property(x => x.EffectiveFrom).HasColumnName("effective_from").HasColumnType("timestamp");
            e.HasIndex(x => new { x.IntersectionId, x.EffectiveFrom }).HasDatabaseName("ix_configs_intersection_effective_from");
        });
    }
}
