using System;
using Microsoft.OpenApi.Models;

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

        /*
        services.Configure<TrafficDataAnalyticsDbSettings>(
            _configuration.GetSection("DefaultConnection")
        );
        services.AddSingleton<TrafficDataAnalyticsDbContext>();
        */

        /******* [2] Repositories ********/

        //services.AddScoped(typeof(INotificationRepository), typeof(NotificationRepository));

        /******* [3] Services ********/


        //services.AddScoped(typeof(IEmailService), typeof(EmailService));

        //services.AddScoped(typeof(INotificationService), typeof(NotificationService));

        /******* [4] AutoMapper ********/

        //services.AddAutoMapper(typeof(NotificationStoreProfile));

        /******* [5] MassTransit ********/

        /*
        services.AddMassTransit(x =>
        {
            x.AddConsumer<NotificationRequestConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                // Configure RabbitMQ connection settings
                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
            
                });

                // user.notifications.request
                cfg.Message<NotificationRequest>(e =>
                {
                    e.SetEntityName(rabbitmqSettings["UserNotificationsExchange"]);
                });
                cfg.Publish<NotificationRequest>(e =>
                {
                    e.ExchangeType = ExchangeType.Direct;
                });
            
                cfg.ReceiveEndpoint("user.notifications.request.queue", e =>
                {
                    e.ConfigureConsumer<NotificationRequestConsumer>(context);
                    e.PrefetchCount = 16;
                    e.UseConcurrencyLimit(4);

                    e.Bind(rabbitmqSettings["UserNotificationsExchange"], x =>
                    {
                        x.ExchangeType = ExchangeType.Direct;
                        x.RoutingKey = rabbitmqSettings["RoutingKeys:UserNotifications:Request"];
                    });

                });

                cfg.ConfigureEndpoints(context);

            });
        });
        */

        /******* [6] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [7] Swagger ********/

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

        //app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}