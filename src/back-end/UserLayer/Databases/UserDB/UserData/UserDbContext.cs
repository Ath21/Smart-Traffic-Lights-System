using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserData.Entities;
using UserData.Settings;

namespace UserData;

public class UserDbContext : DbContext
{
    private readonly UserDbSettings _settings;

    public UserDbContext(DbContextOptions<UserDbContext> options, IOptions<UserDbSettings> settings)
        : base(options)
    {
        _settings = settings.Value;
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }
    public DbSet<UserAuditEntity> UserAudits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → Sessions (1:N)
        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → Audits (1:N)
        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.Audits)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserEntity>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<SessionEntity>()
            .Property(s => s.LoginTime)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<UserAuditEntity>()
            .Property(a => a.Timestamp)
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
