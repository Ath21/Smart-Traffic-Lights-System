using DetectionData;
using MassTransit;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using SensorMessages.Data;
using SensorMessages.Logs;
using EmergencyVehicleDetectionService.Middleware;
using EmergencyVehicleDetectionService.Publishers;
using EmergencyVehicleDetectionStore.Business;
using EmergencyVehicleDetectionStore.Consumers;
using EmergencyVehicleDetectionStore.Publishers;
using EmergencyVehicleDetectionStore.Repositories;
using VehicleDetectionStore.Workers;

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

        services.AddScoped(typeof(IEmergencyVehicleDetectionRepository), typeof(EmergencyVehicleDetectionRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(IEmergencyVehicleDetectionService), typeof(EmergencyVehicleDetectionService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(EmergencyVehicleDetectionStoreProfile));

        /******* [5] MassTransit ********/

        services.AddScoped(typeof(IEmergencyVehicleDetectionPublisher), typeof(EmergencyVehicleDetectionPublisher));
        services.AddScoped<EmergencyVehicleCountConsumer>();

        services.AddMassTransit(x =>
        {
            // Register the consumer for vehicle count messages
            x.AddConsumer<EmergencyVehicleCountConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_configuration["RabbitMQ:Host"],
                        "/",
                        h =>
                {
                    h.Username(_configuration["RabbitMQ:Username"]);
                    h.Password(_configuration["RabbitMQ:Password"]);
                });

                cfg.Message<VehicleCountMessage>(x =>
                {
                    x.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"] ?? "sensor.data.exchange");
                });
                cfg.Publish<VehicleCountMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Topic;
                });

                // Receive endpoint for vehicle count queue
                cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorEmergencyVehicleDetectionVehicleCountQueue"], e =>
                {
                    e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                    {
                        s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorEmergencyVehicleDetectionVehicleCountKey"];
                        s.ExchangeType = "topic";
                    });

                    e.ConfigureConsumer<EmergencyVehicleCountConsumer>(context);
                });

                cfg.Message<AuditLogMessage>(x =>
                {
                    x.SetEntityName(_configuration["RabbitMQ:Exchange:LogStoreExchange"] ?? "log.store.exchange");
                });
                cfg.Publish<AuditLogMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Direct;
                });
                cfg.Message<ErrorLogMessage>(x =>
                {
                    x.SetEntityName(_configuration["RabbitMQ:Exchange:LogStoreExchange"] ?? "log.store.exchange");
                });
                cfg.Publish<ErrorLogMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Direct;
                });
            });
        });

        
        /******* [7] Workers ********/

        services.AddHostedService<EmergencyVehicleSensor>();

        /******* [8] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Emergency Vehicle Detection Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Emergency Vehicle Detection Service API");
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
