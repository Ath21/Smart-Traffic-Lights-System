using System.Text;
using DetectionData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using VehicleDetectionService.Middleware;
using VehicleDetectionStore.Business;
using VehicleDetectionStore.Repositories;

namespace VehicleDetectionStore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        /******* [1] InfluxDB Context ********/

        var influxSettings = _configuration.GetSection("DetectionDb").Get<InfluxDbSettings>();
        services.AddSingleton(influxSettings);
        services.AddSingleton<DetectionDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(IVehicleDetectionRepository), typeof(VehicleDetectionRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(IVehicleDetectService), typeof(VehicleDetectService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(VehicleDetectionStoreProfile));

        /******* [5] MassTransit ********/

        /*
        services.AddScoped(typeof(IUserLogPublisher), typeof(UserLogPublisher));

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
                });

                // Publisher for LogInfo
                cfg.Message<LogInfo>(e => { e.SetEntityName(rabbitmqSettings["UserLogsExchange"]); }); 
                cfg.Publish<LogInfo>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.Message<LogAudit>(e => { e.SetEntityName(rabbitmqSettings["UserLogsExchange"]); });
                cfg.Publish<LogAudit>(e => { e.ExchangeType = ExchangeType.Direct;});

                cfg.Message<LogError>(e => { e.SetEntityName(rabbitmqSettings["UserLogsExchange"]); });
                cfg.Publish<LogError>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.Message<NotificationRequest>(e => { e.SetEntityName(rabbitmqSettings["UserNotificationsExchange"]); });
                cfg.Publish<NotificationRequest>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.ConfigureEndpoints(context);
            });
        });
        */

        /******* [6] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [7] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vehicle Detection Service API", Version = "v1.0" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insert JWT token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicle Detection Service API");
            });
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
