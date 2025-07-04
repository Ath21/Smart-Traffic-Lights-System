/*
 *  LogStore.Startup
 *
 *  This class is responsible for configuring services and middleware for the LogStore application.
 *  It sets up the following services:
 *  
 *  - MongoDB: For storing and retrieving log data.
 *  - Repositories: For data access abstraction (ILogRepository).
 *  - Business Services: For log-related business logic (ILogService).
 *  - AutoMapper: For mapping between data models and DTOs.
 *  - MassTransit: For message-based communication using RabbitMQ, with consumers for info, audit, and error logs.
 *  - Controllers: For handling HTTP API requests.
 *  - Swagger: For API documentation and testing.
 *  
 *  The class also configures the HTTP request pipeline, including exception handling and authentication.
 *  The application uses RabbitMQ for message brokering, with specific configurations for handling user logs
 *  such as info, audit, and error logs.
 *  The application is designed to run in a web environment, handling requests and responses
 *  through ASP.NET Core's middleware pipeline.
 */
using LogData;
using LogStore.Business;
using LogStore.Consumers.User;
using LogStore.Middleware;
using LogStore.Repository;
using MassTransit;
using UserMessages;
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

                // Configure RabbitMQ connection settings
                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
            
                });

                // user.logs.info 
                cfg.Message<LogInfo>(e =>
                {
                    e.SetEntityName(rabbitmqSettings["UserLogsExchange"]);

                });

                cfg.Publish<LogInfo>(e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                });

                cfg.ReceiveEndpoint("user.logs.info.queue", e =>
                {

                    e.ConfigureConsumer<LogInfoConsumer>(context);
                    e.PrefetchCount = 16;
                    e.UseConcurrencyLimit(4);


                    e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Info"];
                    });

                });

                // user.logs.audit
                cfg.Message<LogAudit>(e =>
                {
                    e.SetEntityName(rabbitmqSettings["UserLogsExchange"]);
                });

                cfg.Publish<LogAudit>(e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                });

                cfg.ReceiveEndpoint("user.logs.audit.queue", e =>
                {
                    e.ConfigureConsumer<LogAuditConsumer>(context);
                    e.PrefetchCount = 16;
                    e.UseConcurrencyLimit(4);

                    e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Audit"];
                    });

                });

                // user.logs.error  
                cfg.Message<LogError>(e =>
                {
                    e.SetEntityName(rabbitmqSettings["UserLogsExchange"]);
                });

                cfg.Publish<LogError>(e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                });

                cfg.ReceiveEndpoint("user.logs.error.queue", e =>
                {
                    e.ConfigureConsumer<LogErrorConsumer>(context);
                    e.PrefetchCount = 16;
                    e.UseConcurrencyLimit(4);

                    e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Error"];
                    });

                });

                cfg.ConfigureEndpoints(context);

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
