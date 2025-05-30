using System;
using LogData;
using LogStore.Business;
using LogStore.Consumers;
using LogStore.Middleware;
using LogStore.Repository;
using MassTransit;

using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace LogStore;

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

        services.Configure<LogDbSettings>(
            _configuration.GetSection("DefaultConnection")
        );
        services.AddSingleton<LogDbContext>();
        
        /******* [2] Repositories ********/

        services.AddScoped(typeof(ILogRepository), typeof(LogRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(ILogService), typeof(LogService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(LogStoreProfile));

        /******* [5] MassTransit ********/

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserLogConsumer>();
            x.AddConsumer<TrafficAnalyticsLogConsumer>();
            x.AddConsumer<TrafficLightControlLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq", "/", h =>
                {
                    h.Username("admin");
                    h.Password("admin123");
                });

                cfg.ReceiveEndpoint("user.logs.*", e =>
                {
                    e.ConfigureConsumer<UserLogConsumer>(context);
                    e.Bind("user.logs.info");
                    e.Bind("user.logs.error");
                    e.Bind("user.logs.audit");
                });

                cfg.ReceiveEndpoint("traffic.analytics.*", e =>
                {
                    e.ConfigureConsumer<TrafficAnalyticsLogConsumer>(context);
                    e.Bind("traffic.analytics.daily_summary");
                    e.Bind("traffic.analytics.congestion_alert");
                });

                cfg.ReceiveEndpoint("traffic.light.control.*", e =>
                {
                    e.ConfigureConsumer<TrafficLightControlLogConsumer>(context);
                    e.Bind("traffic.light.control", x =>
                    {
                        x.ExchangeType = ExchangeType.Topic;
                        x.RoutingKey = "traffic.light.control.*"; // Bind to all routing keys under this topic
                    });
                });
            });
        });

        /******* [6] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [7] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Log Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Log Service API");
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
