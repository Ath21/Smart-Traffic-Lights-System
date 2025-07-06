using System;
using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsService.Middleware;
using TrafficDataAnalyticsStore.Repository;
using TrafficMessages;
using TrafficDataAnalyticsStore.Repository.Cyclist;
using TrafficDataAnalyticsStore.Repository.Pedestrian;
using TrafficDataAnalyticsStore.Repository.Vehicle;
using TrafficDataAnalyticsStore.Repository.Congestion;
using TrafficDataAnalyticsStore.Repository.Summary;
using TrafficDataAnalyticsStore.Business.DailySum;
using TrafficDataAnalyticsStore.Business.CongestionDetection;

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

        services.AddDbContext<TrafficDataAnalyticsDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(IDailySummaryRepository), typeof(DailySummaryRepository));
        services.AddScoped(typeof(ICongestionAlertRepository), typeof(CongestionAlertRepository));
        services.AddScoped(typeof(IVehicleCountRepository), typeof(VehicleCountRepository));
        services.AddScoped(typeof(IPedestrianCountRepository), typeof(PedestrianCountRepository));
        services.AddScoped(typeof(ICyclistCountRepository), typeof(CyclistCountRepository));
        
        /******* [3] Services ********/

        services.AddScoped(typeof(IDailyAggregationService), typeof(DailyAggregationService));
        services.AddScoped(typeof(ICongestionDetector), typeof(CongestionDetector));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(TrafficDataAnalyticsStoreProfile));

        /******* [6] Background Services ********/
        
        services.AddHostedService<DailyAggregationJob>();

        /******* [7] MassTransit ********/


        services.AddMassTransit(x =>
        {
            //x.AddConsumer<NotificationRequestConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                // Configure RabbitMQ connection settings
                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
            
                });

                cfg.Message<TrafficDailySummary>(e => { e.SetEntityName(rabbitmqSettings["TrafficDataAnalyticsExchange"]); }); 
                cfg.Publish<TrafficDailySummary>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.Message<TrafficCongestionAlert>(e => { e.SetEntityName(rabbitmqSettings["TrafficDataAnalyticsExchange"]); }); 
                cfg.Publish<TrafficCongestionAlert>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.ConfigureEndpoints(context);

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Data Analytics API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Data Analytics API");
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