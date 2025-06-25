/*
 * NotificationStore.Startup
 *
 * This class is part of the Notification Store service, which is responsible for handling notifications
 * in the system. It configures the services and middleware for the Notification Store application.
 * The Startup class includes methods for configuring services, such as MongoDB, repositories, services,
 * AutoMapper, MassTransit for message queuing, and Swagger for API documentation.
 * It includes the following sections:
 *      1. MongoDb Config: Configures the MongoDB connection settings for the Notification Store service.
 *      2. Repositories: Registers the repositories for data access, specifically the NotificationRepository.
 *      3. Services: Registers the business logic services, including EmailService and NotificationService.
 *      4. AutoMapper: Configures AutoMapper for object-to-object mapping, using the NotificationStoreProfile.
 *      5. MassTransit: Configures MassTransit for message queuing with RabbitMQ,
 *         including the NotificationRequestConsumer for handling notification requests.
 *      6. Controllers: Configures the API controllers and JSON serialization options.
 *      7. Swagger: Configures Swagger for API documentation and testing, including security definitions
 *         for JWT authentication.
 * The Configure method sets up the middleware pipeline for the application, including exception handling,
 * HTTPS redirection, authentication, and authorization. It also sets up the Swagger UI for API
 * documentation.
 * The Startup class is typically used in the NotificationService layer of the application.
 * It is part of the NotificationStore project, which is responsible for managing notifications
 * and related operations in the system.
 */
using MassTransit;
using Microsoft.OpenApi.Models;
using NotificationData;
using NotificationStore.Business.Email;
using NotificationStore.Business.Notify;
using NotificationStore.Consumers;
using NotificationStore.Middleware;
using NotificationStore.Models;
using NotificationStore.Repository;
using RabbitMQ.Client;
using UserMessages;

namespace NotificationStore;

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

        services.Configure<NotificationDbSettings>(
            _configuration.GetSection("DefaultConnection")
        );
        services.AddSingleton<NotificationDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(INotificationRepository), typeof(NotificationRepository));

        /******* [3] Services ********/
       
        services.Configure<EmailSettings>(
            _configuration.GetSection("EmailSettings")
        );
        services.AddScoped(typeof(IEmailService), typeof(EmailService));

        services.AddScoped(typeof(INotificationService), typeof(NotificationService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(NotificationStoreProfile));

        /******* [5] MassTransit ********/

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

        /******* [6] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [7] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API");
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

