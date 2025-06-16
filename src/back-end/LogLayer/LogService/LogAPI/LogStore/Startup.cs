using System;
using LogData;
using LogStore.Business;
using LogStore.Consumers.User;
using LogStore.Consumers.Traffic;
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

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitmqSettings = _configuration.GetSection("RabbitMQ");


        cfg.Host(rabbitmqSettings["Host"], "/", h =>
        {
            h.Username(rabbitmqSettings["Username"]);
            h.Password(rabbitmqSettings["Password"]);
     
        });

        cfg.Message<LogInfo>(e =>
        {
            e.SetEntityName(rabbitmqSettings["UserLogsExchange"]);

        });

        cfg.Publish<LogInfo>(e =>
        {
            e.ExchangeType = ExchangeType.Topic;
        });

        cfg.ReceiveEndpoint("user.logs.info.queue", e =>
        {

            e.ConfigureConsumer<LogInfoConsumer>(context);
            e.PrefetchCount = 16;
            e.UseConcurrencyLimit(4);


            e.Bind(rabbitmqSettings["UserLogsExchange"], x =>
            {
                x.ExchangeType = ExchangeType.Topic;
                x.RoutingKey = rabbitmqSettings["RoutingKeys:UserLogs:Info"];
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
