/*
 * NotificationStore.Program
 *
 * This file is part of the NotificationStore project, which is responsible for handling notifications
 * and related operations in the system.
 * It sets up the application, configures services, and starts the web application.
 * The Program class is the entry point of the application, where the web host is built and run.
 */
namespace NotificationStore
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