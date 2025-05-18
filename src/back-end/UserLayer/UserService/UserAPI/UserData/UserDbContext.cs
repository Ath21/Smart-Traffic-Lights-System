/*
 * UserData.UserDbContext
 *
 * This class represents the database context for the UserData layer.
 * It inherits from DbContext and is responsible for managing the database connection
 * and the entity sets for the application.
 * It contains DbSet properties for the User, Session, and AuditLog entities.
 * The OnConfiguring method sets up the database connection string from the configuration.
 * The OnModelCreating method configures the entity properties and relationships.
 * The UserDbContext constructor takes DbContextOptions and IConfiguration as parameters.
 * The DbContextOptions are used to configure the context, and the IConfiguration is used
 * to retrieve the connection string from the appsettings.json file.
    * The UserDbContext class is used to interact with the database and perform CRUD operations
    * on the User, Session, and AuditLog entities.
    * It is typically used in the UserService layer of the application.
    * The UserDbContext class is part of the UserData layer, which is responsible for data access
    * and management of user-related entities.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserData.Entities;

namespace UserData;

public class UserDbContext : DbContext
{
    public IConfiguration _configuration { get; }

    public UserDbContext(DbContextOptions<UserDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .Property(u => u.Status)
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // AuditLog entity configuration
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
