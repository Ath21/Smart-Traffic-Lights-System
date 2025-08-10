using System;
using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

using TrafficMessages;


namespace TrafficDataAnalyticsStore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        /******* [1] PostgreSQL Config ********/

        //services.AddDbContext<TrafficDataAnalyticsDbContext>();

        /******* [2] Repositories ********/

        /*    
        services.AddScoped(typeof(IDailySummaryRepository), typeof(DailySummaryRepository));
        services.AddScoped(typeof(ICongestionAlertRepository), typeof(CongestionAlertRepository));
        services.AddScoped(typeof(IVehicleCountRepository), typeof(VehicleCountRepository));
        services.AddScoped(typeof(IPedestrianCountRepository), typeof(PedestrianCountRepository));
        services.AddScoped(typeof(ICyclistCountRepository), typeof(CyclistCountRepository));
        */
        
        /******* [3] Services ********/

        //services.AddScoped(typeof(IDailyAggregationService), typeof(DailyAggregationService));
        //services.AddScoped(typeof(ICongestionDetector), typeof(CongestionDetector));

        /******* [4] AutoMapper ********/

        //services.AddAutoMapper(typeof(TrafficDataAnalyticsStoreProfile));

        /******* [6] Background Services ********/
        
        //services.AddHostedService<DailyAggregationJob>();

        /******* [7] MassTransit ********/


        services.AddScoped(typeof(ICyclistDetectionPublisher), typeof(CyclistDetectionPublisher));
        services.AddScoped<CyclistDetectionConsumer>();

        services.AddMassTransit(x =>
        {
            // Register the consumer for vehicle count messages
            x.AddConsumer<CyclistDetectionConsumer>();

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
                cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:SensorCyclistDetectionRequestQueue"], e =>
                {
                    e.Bind(_configuration["RabbitMQ:Exchange:SensorDataExchange"], s =>
                    {
                        s.RoutingKey = _configuration["RabbitMQ:RoutingKey:SensorCyclistDetectionRequestKey"];
                        s.ExchangeType = "topic";
                    });

                    e.ConfigureConsumer<CyclistDetectionConsumer>(context);
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
        

        /******* [7] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intersection Control API");
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