using System;
using DetectionData;
using MassTransit;
using Microsoft.OpenApi.Models;
using PedestrianDetectionStore;
using PedestrianDetectionStore.Business;
using PedestrianDetectionStore.Middleware;
using PedestrianDetectionStore.Repositories;
using RabbitMQ.Client;
using SensorMessages.Data;
using SensorMessages.Logs;

namespace PublicTransportDetectionStore;

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

        services.AddScoped(typeof(IPedestrianDetectionRepository), typeof(PedestrianDetectionRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(IPedestrianDetectService), typeof(PedestrianDetectService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(PedestrianDetectionStoreProfile));

        /******* [5] MassTransit ********/

        services.AddScoped(typeof(IPedestrianDetectionPublisher), typeof(PedestrianDetectionPublisher));
        services.AddScoped<PedestrianDetectionConsumer>();

        services.AddMassTransit(x =>
        {
            // Register the consumer for vehicle count messages
            x.AddConsumer<PedestrianDetectionConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_configuration["RabbitMQ:Host"],
                        "/",
                        h =>
                {
                    h.Username(_configuration["RabbitMQ:Username"]);
                    h.Password(_configuration["RabbitMQ:Password"]);
                });

                cfg.Message<PublicTransportDetectionMessage>(x =>
                {
                    x.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                });
                cfg.Publish<PublicTransportDetectionMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Topic;
                });

                // Receive endpoint for vehicle count queue
                cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorPedestrianDetectionRequestQueue"], e =>
                {
                    e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                    {
                        s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorPedestrianDetectionRequestKey"];
                        s.ExchangeType = "topic";
                    });

                    e.ConfigureConsumer<PedestrianDetectionConsumer>(context);
                });

                cfg.Message<AuditLogMessage>(x =>
                {
                    x.SetEntityName(_configuration["RabbitMQ:Exchange:LogStoreExchange"]);
                });
                cfg.Publish<AuditLogMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Direct;
                });
                cfg.Message<ErrorLogMessage>(x =>
                {
                    x.SetEntityName(_configuration["RabbitMQ:Exchange:LogStoreExchange"]);
                });
                cfg.Publish<ErrorLogMessage>(x =>
                {
                    x.ExchangeType = ExchangeType.Direct;
                });
            });
        });

        
        /******* [7] Workers ********/

        services.AddHostedService<PedestrianSensor>();

        /******* [8] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pedestrian Detection Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pedestrian Detection Service API");
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
