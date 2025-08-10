using System;
using IntersectionControlStore.Middleware;
using IntersectionControlStore.Publishers.LightPub;
using IntersectionControlStore.Publishers.LogPub;
using IntersectionControlStore.Publishers.PriorityPub;
using IntersectionControlStore.Consumers; // assuming your consumers are in this namespace
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using TrafficMessages.Logs;
using TrafficLightControlService.Consumers;
using TrafficMessages.Priority;
using TrafficMessages.Light;
using SensorMessages.Data;

namespace IntersectionControlStore
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            /******* [1] Database Config ********/
            // Example: services.AddDbContext<YourDbContext>(...);

            /******* [2] Repositories ********/
            // Register repositories here if any:
            // services.AddScoped<IYourRepository, YourRepository>();

            /******* [3] Services ********/
            // Register any domain services here:
            // services.AddScoped<IYourService, YourService>();

            /******* [4] AutoMapper ********/
            // services.AddAutoMapper(typeof(YourMappingProfile));

            /******* [5] Publishers ********/
            services.AddScoped<IPriorityPublisher, PriorityPublisher>();
            services.AddScoped<ITrafficLightControlPublisher, TrafficLightControlPublisher>();
            services.AddScoped<ITrafficLogPublisher, TrafficLogPublisher>();

            /******* [6] Consumers ********/
            // Register your consumers for MassTransit here:
            services.AddScoped<TrafficLightStateUpdateConsumer>();
            services.AddScoped<SensorDataConsumer>();
            // Add other consumers as needed

            /******* [7] MassTransit & RabbitMQ Config ********/
            services.AddMassTransit(x =>
            {
                x.AddConsumer<SensorDataConsumer>();
                x.AddConsumer<TrafficLightStateUpdateConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(_configuration["RabbitMQ:Username"]);
                        h.Password(_configuration["RabbitMQ:Password"]);
                    });

                    // Sensor Data Exchange setup
                    cfg.Message<VehicleCountMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                    });
                    cfg.Message<EmergencyVehicleDetectionMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                    });
                    cfg.Message<PublicTransportDetectionMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                    });
                    cfg.Message<PedestrianDetectionMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                    });
                    cfg.Message<CyclistDetectionMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                    });
                    cfg.Message<IncidentDetectionMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:SensorDataExchange"]);
                    });

                    // Publish exchange type for all sensor messages
                    cfg.Publish<VehicleCountMessage>(m => m.ExchangeType = ExchangeType.Topic);
                    cfg.Publish<EmergencyVehicleDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
                    cfg.Publish<PublicTransportDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
                    cfg.Publish<PedestrianDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
                    cfg.Publish<CyclistDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
                    cfg.Publish<IncidentDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);

                    // Receive endpoints - one per sensor data queue + routing key

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorVehicleCountQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorVehicleCount"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<SensorDataConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorEmergencyVehicleQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorEmergencyVehicle"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<SensorDataConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorPublicTransportQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorPublicTransport"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<SensorDataConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorPedestrianQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorPedestrian"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<SensorDataConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorCyclistQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorCyclist"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<SensorDataConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorIncidentQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorIncident"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<SensorDataConsumer>(context);
                    });

                    // Traffic Light Coordination Exchange & Consumer

                    cfg.Message<TrafficLightStateUpdate>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:TrafficControlExchange"]);
                    });
                    cfg.Publish<TrafficLightStateUpdate>(m =>
                    {
                        m.ExchangeType = ExchangeType.Topic;
                    });

                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:TrafficLightStateUpdateQueue"], e =>
                    {
                        e.Bind(_configuration["RabbitMQ:Exchange:TrafficControlExchange"], s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:TrafficLightCoordinationUpdate"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<TrafficLightStateUpdateConsumer>(context);
                    });

                    // Logs exchange setup (optional)

                    cfg.Message<AuditLogMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:LogStoreExchange"]);
                    });
                    cfg.Publish<AuditLogMessage>(m =>
                    {
                        m.ExchangeType = ExchangeType.Direct;
                    });
                    cfg.Message<ErrorLogMessage>(m =>
                    {
                        m.SetEntityName(_configuration["RabbitMQ:Exchange:LogStoreExchange"]);
                    });
                    cfg.Publish<ErrorLogMessage>(m =>
                    {
                        m.ExchangeType = ExchangeType.Direct;
                    });
                });
            });



            /******* [8] Controllers ********/
            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            /******* [9] Exception Middleware ********/
            // Middleware will be added in Configure method

            /******* [10] Swagger ********/
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Intersection Control API", Version = "v1.0" });
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
                        Array.Empty<string>()
                    }
                });
            });

            /******* [11] Authentication & Authorization ********/
            // Configure your authentication here
            // services.AddAuthentication(...);
            // services.AddAuthorization(...);
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intersection Control API");
                });
            }

            app.UseHttpsRedirection();

            // Exception Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // Authentication & Authorization Middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
