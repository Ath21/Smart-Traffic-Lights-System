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
            "Server=user_db,1433;Database=UserDB;User Id=sa;Password=MyPass@word;TrustServerCertificate=True");

        return new UserDbContext(optionsBuilder.Options, new ConfigurationBuilder().Build());
    }
}

