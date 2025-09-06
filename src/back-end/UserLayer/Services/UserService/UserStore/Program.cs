/*
 * UserStore.Program
 *
 * This class is the entry point for the UserStore application.
 * It sets up the web application using ASP.NET Core.
 * The Main method creates a new web application builder, configures services, and builds the application.
 * The Startup class is used to configure services and middleware for the application.
 * The application is then run using the built-in web server.
 * The Program class is typically used in ASP.NET Core applications to set up the application host and configure the request pipeline.
 * It is part of the UserService layer, which is responsible for handling user-related operations.
 */
namespace UserStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);
            var app = builder.Build();
            startup.Configure(app, builder.Environment);
        }
    }
}