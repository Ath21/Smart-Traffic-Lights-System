/*
 * UserData.UserDbContextFactory
 *
 * This class is responsible for creating instances of the UserDbContext
    * for design-time services, such as migrations and database updates.
    * It implements the IDesignTimeDbContextFactory interface.
    * The CreateDbContext method is used to create a new instance of the UserDbContext
    * with the specified options.
    * It uses the DbContextOptionsBuilder to configure the context with a SQL Server database connection.
    * The connection string is hardcoded for design-time purposes.
    * In a production environment, the connection string should be stored in a secure location,
    * such as environment variables or a configuration file.
    * The UserDbContextFactory class is typically used in the UserService layer of the application.
    * It is part of the UserData layer, which is responsible for data access
    * and management of user-related entities.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserData;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=user_mssql,1433;Database=UserDb;User Id=sa;Password=MyPass@word;TrustServerCertificate=True");

        return new UserDbContext(optionsBuilder.Options, new ConfigurationBuilder().Build());
    }
}

