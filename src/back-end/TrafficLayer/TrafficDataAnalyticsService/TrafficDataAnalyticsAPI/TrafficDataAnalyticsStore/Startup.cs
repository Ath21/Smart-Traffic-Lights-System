using System;
using MassTransit;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Redis;
using TrafficDataAnalyticsService.Middleware;
using TrafficDataAnalyticsStore.Business.Congestion;
using TrafficDataAnalyticsStore.Business.DailySum;
using TrafficDataAnalyticsStore.Business.RedisReader;
using TrafficDataAnalyticsStore.Repository;
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
        /******* [1] MongoDb Config ********/

        
        services.Configure<TrafficDataAnalyticsDbSettings>(
            _configuration.GetSection("DefaultConnection")
        );
        services.AddSingleton<TrafficDataAnalyticsDbContext>();

        /******* [2] Redis Config ********/

        services.Configure<RedisDbSettings>(
            _configuration.GetSection("RedisConnection")
        );
        services.AddSingleton<RedisDbContext>();

        /******* [3] Repositories ********/

        services.AddScoped(typeof(IMongoDbWriter), typeof(MongoDbWriter));

        /******* [4] Services ********/

        services.AddScoped(typeof(IRedisReader), typeof(RedisReader));
        services.AddScoped(typeof(ISummaryService), typeof(SummaryService));
        services.AddScoped(typeof(ICongestionAlertService), typeof(CongestionAlertService));

        /******* [5] AutoMapper ********/

        services.AddAutoMapper(typeof(TrafficDataAnalyticsStoreProfile));

        /******* [6] MassTransit ********/

        
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