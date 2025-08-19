using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorData;

public class TrafficLightCoordinatorDbContext : DbContext
{
    public TrafficLightCoordinatorDbContext(DbContextOptions<TrafficLightCoordinatorDbContext> options) : base(options) { }

    public DbSet<Intersection> Intersections => Set<Intersection>();
    public DbSet<TrafficLight> TrafficLights => Set<TrafficLight>();
    public DbSet<TrafficConfiguration> TrafficConfigurations => Set<TrafficConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Intersection>()
            .HasMany(i => i.Lights).WithOne(l => l.Intersection!)
            .HasForeignKey(l => l.IntersectionId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Intersection>()
            .HasMany(i => i.Configurations).WithOne(c => c.Intersection!)
            .HasForeignKey(c => c.IntersectionId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Intersection>().HasIndex(i => i.Status);
        modelBuilder.Entity<TrafficLight>().HasIndex(l => new { l.IntersectionId, l.UpdatedAt });
        modelBuilder.Entity<TrafficConfiguration>().HasIndex(c => new { c.IntersectionId, c.UpdatedAt });
    }
}
