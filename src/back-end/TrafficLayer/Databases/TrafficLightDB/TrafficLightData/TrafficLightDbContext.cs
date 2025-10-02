using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrafficLightData.Entities;

namespace TrafficLightData;

public class TrafficLightDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public TrafficLightDbContext(
        DbContextOptions<TrafficLightDbContext> options,
        IConfiguration? configuration = null) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Intersection> Intersections => Set<Intersection>();
    public DbSet<TrafficLight> TrafficLights => Set<TrafficLight>();
    public DbSet<TrafficConfiguration> TrafficConfigurations => Set<TrafficConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =============================
        // INTERSECTIONS
        // =============================
        modelBuilder.Entity<Intersection>(e =>
        {
            e.HasKey(i => i.IntersectionId);

            e.Property(i => i.IntersectionId)
                .ValueGeneratedOnAdd();

            e.HasMany(i => i.TrafficLights)
                .WithOne(l => l.Intersection!)
                .HasForeignKey(l => l.IntersectionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(i => i.Configurations)
                .WithOne(c => c.Intersection!)
                .HasForeignKey(c => c.IntersectionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(i => i.Status);
        });

        // =============================
        // TRAFFIC LIGHTS
        // =============================
        modelBuilder.Entity<TrafficLight>(e =>
        {
            e.HasKey(l => l.LightId);

            e.Property(l => l.LightId)
                .ValueGeneratedOnAdd();

            e.HasIndex(l => new { l.IntersectionId, l.UpdatedAt });

            e.Property(l => l.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Store enum as string in SQL Server
            e.Property(l => l.CurrentState)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.ToTable(t => t.HasCheckConstraint("ck_traffic_lights_state",
                "current_state IN ('RED','ORANGE','GREEN','FLASHING','OFF')"));
        });

        // =============================
        // TRAFFIC CONFIGURATIONS
        // =============================
        modelBuilder.Entity<TrafficConfiguration>(e =>
        {
            e.HasKey(c => c.ConfigId);

            e.Property(c => c.ConfigId)
                .ValueGeneratedOnAdd();

            e.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration is not null)
        {
            var connectionString = _configuration.GetConnectionString("MSSQLConnection");
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
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
