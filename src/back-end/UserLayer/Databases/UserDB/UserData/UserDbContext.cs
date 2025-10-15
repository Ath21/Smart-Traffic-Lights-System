using System.Security.Cryptography;
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
        // Relationships
        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.Audits)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Default timestamps
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<SessionEntity>()
            .Property(s => s.LoginTime)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<UserAuditEntity>()
            .Property(a => a.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        // Seed Users
        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity
            {
                UserId = 1,
                Username = "vathanasiou",
                Email = "ice1939005@gmail.com",
                PasswordHash = HashPassword("qaz123!@#"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new UserEntity
            {
                UserId = 2,
                Username = "vmamalis",
                Email = "billath131908@gmail.com",
                PasswordHash = HashPassword("vmamalis123!@#"),
                Role = "TrafficOperator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new UserEntity
            {
                UserId = 3,
                Username = "apostolos",
                Email = "athinnovations@gmail.com",
                PasswordHash = HashPassword("apostolos123!@#"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

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

    private string HashPassword(string password)
    {
        const int SaltSize = 16;
        const int KeySize = 32;
        const int Iterations = 10000;

        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
        
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }
}
