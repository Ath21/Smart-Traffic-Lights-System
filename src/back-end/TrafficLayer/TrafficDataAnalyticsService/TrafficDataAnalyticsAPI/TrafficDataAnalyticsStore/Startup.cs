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
using TrafficDataAnalyticsStore.Repository.Summary;
using TrafficDataAnalyticsStore.Business.DailySum;
using TrafficDataAnalyticsStore.Business.CongestionDetection;
using TrafficDataAnalyticsStore.Repository.Alerts;
using TrafficDataAnalyticsStore.Publishers.Congestion;
using TrafficDataAnalyticsStore.Publishers.Incident;
using TrafficDataAnalyticsStore.Publishers.Summary;
using TrafficDataAnalyticsStore.Publishers.Logs;
using TrafficDataAnalyticsStore.Consumers;
using TrafficDataAnalyticsStore.Business;

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
        services.AddScoped(typeof(IAlertRepository), typeof(AlertRepository));
        
        /******* [3] Services ********/

        services.AddScoped(typeof(ITrafficAnalyticsService), typeof(TrafficAnalyticsService));
        
        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(TrafficDataAnalyticsStoreProfile));

        /******* [5] Publishers ********/

        services.AddScoped(typeof(ITrafficCongestionPublisher), typeof(TrafficCongestionPublisher));
        services.AddScoped(typeof(ITrafficIncidentPublisher), typeof(TrafficIncidentPublisher));
        services.AddScoped(typeof(ITrafficSummaryPublisher), typeof(TrafficSummaryPublisher));
        services.AddScoped(typeof(IAnalyticsLogPublisher), typeof(AnalyticsLogPublisher));

        /******* [6] Consumers ********/

        services.AddScoped(typeof(EmergencyVehicleConsumer));
        services.AddScoped(typeof(PublicTransportConsumer));
        services.AddScoped(typeof(PedestrianDetectionConsumer));
        services.AddScoped(typeof(CyclistDetectionConsumer));
        services.AddScoped(typeof(IncidentDetectionConsumer));

        /******* [7] MassTransit *******/

        services.AddTrafficAnalyticsMassTransit(_configuration);

        /******* [8] Controllers ********/

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