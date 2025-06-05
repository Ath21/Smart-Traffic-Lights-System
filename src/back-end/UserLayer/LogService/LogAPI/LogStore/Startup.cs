using System;
using LogData;
using LogStore.Business;
using LogStore.Consumers.User;
using LogStore.Consumers.Traffic;
using LogStore.Middleware;
using LogStore.Repository;
using MassTransit;

using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using LogStore.Messages.User;
using LogStore.Messages.Traffic;

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
            x.AddConsumer<LogInfoConsumer>();
            x.AddConsumer<LogAuditConsumer>();
            x.AddConsumer<LogErrorConsumer>();
            x.AddConsumer<TrafficAnalyticsLogConsumer>();
            x.AddConsumer<TrafficCongestionAlertConsumer>();
            x.AddConsumer<TrafficLightControlLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
                });

                // USER LOGS - Separate endpoints for each log type
                cfg.ReceiveEndpoint("user.logs", e =>
                {
                    e.ConfigureConsumer<LogInfoConsumer>(context);
                    e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Info"];
                        x.ExchangeType = ExchangeType.Direct;
                    });

                    e.ConfigureConsumer<LogAuditConsumer>(context);
                    e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Audit"];
                        x.ExchangeType = ExchangeType.Direct;
                    });

                    e.ConfigureConsumer<LogErrorConsumer>(context);
                    e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Error"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                });

                // TRAFFIC ANALYTICS
                cfg.ReceiveEndpoint("traffic.analytics", e =>
                {
                    e.ConfigureConsumer<TrafficAnalyticsLogConsumer>(context);
                    e.ConfigureConsumer<TrafficCongestionAlertConsumer>(context);

                    e.Bind(rabbitmqSettings["TrafficAnalyticsExchange"], x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:Traffic:DailySummary"];
                        x.ExchangeType = ExchangeType.Direct;
                    });

                    e.Bind(rabbitmqSettings["TrafficAnalyticsExchange"], x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:Traffic:CongestionAlert"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                });

                // TRAFFIC LIGHT CONTROL
                cfg.ReceiveEndpoint("traffic.light.control", e =>
                {
                    e.ConfigureConsumer<TrafficLightControlLogConsumer>(context);

                    e.Bind(rabbitmqSettings["TrafficLightControlExchange"], x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:Traffic:LightControlPattern"];
                        x.ExchangeType = ExchangeType.Topic;
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
