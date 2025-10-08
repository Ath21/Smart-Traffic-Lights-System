using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationData;
using NotificationData.Repositories.DeliveryLogs;
using NotificationData.Repositories.Notifications;
using NotificationData.Settings;
using NotificationStore.Business.Email;
using NotificationStore.Business.Notify;
using NotificationStore.Consumers;
using NotificationStore.Middleware;
using NotificationStore.Models;
using NotificationStore.Publishers.Logs;
using NotificationStore.Publishers.Notifications;

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
        // ===============================
        // Data Layer (MongoDB - NotificationDB)
        // ===============================
        // Db Context
        services.Configure<NotificationDbSettings>(options =>
        {
            options.ConnectionString = _configuration["Mongo:ConnectionString"];
            options.Database = _configuration["Mongo:Database"];
            options.Collections = new CollectionsSettings
            {
                Notifications = _configuration["Mongo:Collections:Notifications"],
                DeliveryLogs = _configuration["Mongo:Collections:DeliveryLogs"]
            };
        });
        services.AddSingleton<NotificationDbContext>();

        // Repositories
        services.AddScoped(typeof(INotificationRepository), typeof(NotificationRepository));
        services.AddScoped(typeof(IDeliveryLogRepository), typeof(DeliveryLogRepository));

        // ===============================
        // Business Layer (Services)
        // ===============================
        // Email Service
        services.Configure<EmailSettings>(options =>
        {
            options.SmtpServer = _configuration["Email:SmtpServer"];
            options.Port = int.Parse(_configuration["Email:Port"] ?? "587");
            options.SenderName = _configuration["Email:SenderName"];
            options.SenderEmail = _configuration["Email:SenderEmail"];
            options.Username = _configuration["Email:Username"];
            options.Password = _configuration["Email:Password"];
        });
        services.AddScoped(typeof(IEmailService), typeof(EmailService));

        // Notification Service
        services.AddScoped(typeof(INotificationService), typeof(NotificationService));

        // ===============================
        // AutoMapper
        // ===============================
        services.AddAutoMapper(typeof(NotificationStoreProfile));

        // ===============================
        // Message Layer (MassTransit with RabbitMQ)
        // ===============================
        // Publishers
        services.AddScoped(typeof(INotificationPublisher), typeof(NotificationPublisher));
        services.AddScoped(typeof(INotificationLogPublisher), typeof(NotificationLogPublisher));

        // Consumers
        services.AddScoped<NotificationRequestConsumer>();
        services.AddScoped<TrafficIncidentConsumer>();
        services.AddScoped<TrafficCongestionConsumer>();
        services.AddScoped<TrafficSummaryConsumer>();

        // MassTransit Setup
        services.AddNotificationServiceMassTransit(_configuration);

        // ===============================
        // JWT Authentication
        // ===============================
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        // ===============================
        // CORS Policy
        // ===============================
        var allowedOrigins = _configuration["Cors:AllowedOrigins"]?.Split(",") ?? Array.Empty<string>();
        var allowedMethods = _configuration["Cors:AllowedMethods"]?.Split(",") ?? new[] { "GET", "POST", "PUT", "PATCH", "DELETE" };
        var allowedHeaders = _configuration["Cors:AllowedHeaders"]?.Split(",") ?? new[] { "Content-Type", "Authorization" };

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods(allowedMethods)
                      .WithHeaders(allowedHeaders);
            });
        });

        // ===============================
        // Controllers
        // ===============================
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        // ===============================
        // Swagger (API Documentation)
        // ===============================
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Notification Service",
                Version = "v3.0",
                Description = "Centralized cloud service for broadcasting alerts, notifications, and email updates across the UNIWA STLS ecosystem."
            });

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
                    Array.Empty<string>()
                }
            });
        });
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        // ===============================
        // Swagger UI
        // ===============================
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service");
                c.DocumentTitle = "Notification Service";
            });
        }

        // ===============================
        // Middleware
        // ===============================
        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        // ===============================
        // Endpoints
        // ===============================
        app.MapControllers();

        app.Run();
    }
}
