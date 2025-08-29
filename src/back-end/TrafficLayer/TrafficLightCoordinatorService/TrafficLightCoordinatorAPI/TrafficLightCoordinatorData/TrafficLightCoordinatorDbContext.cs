using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorData;

public class TrafficLightCoordinatorDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public TrafficLightCoordinatorDbContext(
        DbContextOptions<TrafficLightCoordinatorDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Intersection> Intersections => Set<Intersection>();
    public DbSet<TrafficLight> TrafficLights => Set<TrafficLight>();
    public DbSet<TrafficConfiguration> TrafficConfigurations => Set<TrafficConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // INTERSECTIONS
        modelBuilder.Entity<Intersection>(e =>
        {
            e.HasMany(i => i.Lights).WithOne(l => l.Intersection!)
                .HasForeignKey(l => l.IntersectionId).OnDelete(DeleteBehavior.Cascade);

            e.HasMany(i => i.Configurations).WithOne(c => c.Intersection!)
                .HasForeignKey(c => c.IntersectionId).OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(i => i.Status);
        });

        // TRAFFIC_LIGHTS
        modelBuilder.Entity<TrafficLight>(e =>
        {
            e.HasIndex(l => new { l.IntersectionId, l.UpdatedAt });
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("(now() at time zone 'utc')");

            e.ToTable(t => t.HasCheckConstraint("ck_traffic_lights_state",
                "current_state IN ('RED','AMBER','GREEN','FLASHING','OFF')"));
        });

        // TRAFFIC_CONFIGURATIONS
        modelBuilder.Entity<TrafficConfiguration>(e =>
        {
            // Fast "latest config" lookup per intersection and time
            e.HasIndex(c => new { c.IntersectionId, c.EffectiveFrom }).HasDatabaseName("ix_cfg_intersection_effective");
            e.HasIndex(c => c.ChangeRef).IsUnique();

            e.Property(c => c.EffectiveFrom).HasDefaultValueSql("(now() at time zone 'utc')");
            e.Property(c => c.CreatedAt).HasDefaultValueSql("(now() at time zone 'utc')");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var cs = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(cs);
        }
    }
}
