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
using IntersectionControlStore.Business;

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
            services.AddScoped(typeof(IPriorityManager), typeof(PriorityManager));

            /******* [4] AutoMapper ********/
            // services.AddAutoMapper(typeof(YourMappingProfile));

            /******* [5] Publishers ********/
            services.AddScoped(typeof(IPriorityPublisher), typeof(PriorityPublisher));
            services.AddScoped(typeof(ITrafficLightControlPublisher), typeof(TrafficLightControlPublisher));
            services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

            /******* [6] Consumers ********/
            // Register your consumers for MassTransit here:
            services.AddScoped(typeof(TrafficLightStateUpdateConsumer));
            services.AddScoped(typeof(SensorDataConsumer));
            // Add other consumers as needed

            /******* [7] MassTransit & RabbitMQ Config ********/
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

        /* ------------------- SENSOR DATA ------------------- */
        string sensorExchange = _configuration["RabbitMQ:Exchange:SensorDataExchange"];

        cfg.Message<VehicleCountMessage>(m => m.SetEntityName(sensorExchange));
        cfg.Message<EmergencyVehicleDetectionMessage>(m => m.SetEntityName(sensorExchange));
        cfg.Message<PublicTransportDetectionMessage>(m => m.SetEntityName(sensorExchange));
        cfg.Message<PedestrianDetectionMessage>(m => m.SetEntityName(sensorExchange));
        cfg.Message<CyclistDetectionMessage>(m => m.SetEntityName(sensorExchange));
        cfg.Message<IncidentDetectionMessage>(m => m.SetEntityName(sensorExchange));

        cfg.Publish<VehicleCountMessage>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<EmergencyVehicleDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<PublicTransportDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<PedestrianDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<CyclistDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<IncidentDetectionMessage>(m => m.ExchangeType = ExchangeType.Topic);

        // Subscribe to sensor data queues
        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:VehicleCountQueue"], e =>
        {
            e.Bind(sensorExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:VehicleCount"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<SensorDataConsumer>(context);
        });

        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:EmergencyVehicleQueue"], e =>
        {
            e.Bind(sensorExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:EmergencyVehicle"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<SensorDataConsumer>(context);
        });

        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:PublicTransportQueue"], e =>
        {
            e.Bind(sensorExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:PublicTransport"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<SensorDataConsumer>(context);
        });

        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:PedestrianRequestQueue"], e =>
        {
            e.Bind(sensorExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:PedestrianRequest"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<SensorDataConsumer>(context);
        });

        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:CyclistRequestQueue"], e =>
        {
            e.Bind(sensorExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:CyclistRequest"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<SensorDataConsumer>(context);
        });

        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:IncidentQueue"], e =>
        {
            e.Bind(sensorExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:Incident"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<SensorDataConsumer>(context);
        });

        /* ------------------- PRIORITY & TRAFFIC LIGHT CONTROL ------------------- */
        string trafficControlExchange = _configuration["RabbitMQ:Exchange:TrafficControlExchange"];

        // Messages published by intersection control (priority & control)
        cfg.Message<PriorityEmergencyVehicle>(m => m.SetEntityName(trafficControlExchange));
        cfg.Message<PriorityPublicTransport>(m => m.SetEntityName(trafficControlExchange));
        cfg.Message<PriorityPedestrian>(m => m.SetEntityName(trafficControlExchange));
        cfg.Message<PriorityCyclist>(m => m.SetEntityName(trafficControlExchange));
        cfg.Message<TrafficLightControl>(m => m.SetEntityName(trafficControlExchange));
        cfg.Message<TrafficLightStateUpdate>(m => m.SetEntityName(trafficControlExchange));

        cfg.Publish<PriorityEmergencyVehicle>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<PriorityPublicTransport>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<PriorityPedestrian>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<PriorityCyclist>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<TrafficLightControl>(m => m.ExchangeType = ExchangeType.Topic);
        cfg.Publish<TrafficLightStateUpdate>(m => m.ExchangeType = ExchangeType.Topic);

        // Receive traffic light state updates
        cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:TrafficLightControlQueue"], e =>
        {
            e.Bind(trafficControlExchange, s =>
            {
                s.RoutingKey = _configuration["RabbitMQ:RoutingKey:TrafficLightControl"];
                s.ExchangeType = ExchangeType.Topic;
            });
            e.ConfigureConsumer<TrafficLightStateUpdateConsumer>(context);
        });

        /* ------------------- LOGGING ------------------- */
        string logExchange = _configuration["RabbitMQ:Exchange:LogStoreExchange"];

        cfg.Message<AuditLogMessage>(m => m.SetEntityName(logExchange));
        cfg.Message<ErrorLogMessage>(m => m.SetEntityName(logExchange));

        cfg.Publish<AuditLogMessage>(m => m.ExchangeType = ExchangeType.Direct);
        cfg.Publish<ErrorLogMessage>(m => m.ExchangeType = ExchangeType.Direct);

        // Assuming logs are only published, no subscription needed here for intersection control
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
