/*
 *  LogStore.Program
 *
 *  This class is the entry point for the LogStore application.
 *  It sets up the web application using ASP.NET Core, configuring services and middleware.
 */
namespace LogStore
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