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
            

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                var host = rabbitmqSettings["Host"];

                var username = rabbitmqSettings["Username"];
                var password = rabbitmqSettings["Password"];

                var userLogsExchange = rabbitmqSettings["UserLogsExchange"];

                var trafficAnalyticsExchange = rabbitmqSettings["TrafficAnalyticsExchange"];
                var trafficLightControlExchange = rabbitmqSettings["TrafficLightControlExchange"];

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // 🔹 USER LOGS
                cfg.ReceiveEndpoint("user.logs", e =>
                {
                    e.ConfigureConsumer<LogInfoConsumer>(context);
                    e.ConfigureConsumer<LogAuditConsumer>(context);
                    e.ConfigureConsumer<LogErrorConsumer>(context);

                    e.Bind(userLogsExchange, x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Info"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                    e.Bind(userLogsExchange, x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Audit"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                    e.Bind(userLogsExchange, x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Error"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                });

                	                  
                // 🔹 TRAFFIC ANALYTICS
                cfg.ReceiveEndpoint("traffic.analytics", e =>
                {
                    e.ConfigureConsumer<TrafficAnalyticsLogConsumer>(context);

                    e.Bind(trafficAnalyticsExchange, x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:Traffic:DailySummary"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                    e.Bind(trafficAnalyticsExchange, x =>
                    {
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:Traffic:CongestionAlert"];
                        x.ExchangeType = ExchangeType.Direct;
                    });
                });
                

                
                // 🔹 TRAFFIC LIGHT CONTROL
                cfg.ReceiveEndpoint("traffic.light.control", e =>
                {
                    e.ConfigureConsumer<TrafficLightControlLogConsumer>(context);

                    e.Bind(trafficLightControlExchange, x =>
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
