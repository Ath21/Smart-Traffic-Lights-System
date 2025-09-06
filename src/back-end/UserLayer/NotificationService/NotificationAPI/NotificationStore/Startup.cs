using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationData;
using NotificationStore.Business.Email;
using NotificationStore.Business.Notify;
using NotificationStore.Consumers;
using NotificationStore.Middleware;
using NotificationStore.Models;
using NotificationStore.Publishers.Logs;
using NotificationStore.Publishers.Notifications;
using NotificationStore.Repositories.DeliveryLogs;
using NotificationStore.Repositories.Notifications;

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
        services.Configure<NotificationDbSettings>(options =>
        {
            options.ConnectionString = _configuration["Mongo:ConnectionString"];
            options.Database = _configuration["Mongo:Database"];
            options.NotificationsCollection = _configuration["Mongo:NotificationsCollection"];
            options.DeliveryLogsCollection = _configuration["Mongo:DeliveryLogsCollection"];
        });
        services.AddSingleton<NotificationDbContext>();

        /******* [2] Repositories ********/
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeliveryLogRepository, DeliveryLogRepository>();

        /******* [3] Services ********/
        services.Configure<EmailSettings>(options =>
        {
            options.SmtpServer = _configuration["Email:SmtpServer"];
            options.Port = int.Parse(_configuration["Email:Port"] ?? "587");
            options.SenderName = _configuration["Email:SenderName"];
            options.SenderEmail = _configuration["Email:SenderEmail"];
            options.Username = _configuration["Email:Username"];
            options.Password = _configuration["Email:Password"];
        });
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        /******* [4] AutoMapper ********/
        services.AddAutoMapper(typeof(NotificationStoreProfile));

        /******* [5] Publishers ********/
        services.AddScoped<INotificationPublisher, NotificationPublisher>();
        services.AddScoped<INotificationLogPublisher, NotificationLogPublisher>();

        /******* [6] Consumers ********/
        services.AddScoped<NotificationRequestConsumer>();
        services.AddScoped<TrafficIncidentConsumer>();
        services.AddScoped<TrafficCongestionConsumer>();
        services.AddScoped<TrafficSummaryConsumer>();

        /******* [7] MassTransit ********/
        services.AddNotificationServiceMassTransit(_configuration);

        /******* [8] Jwt Config ********/
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

        /******* [9] CORS Policy ********/
        var allowedOrigins = _configuration["Cors:AllowedOrigins"]?.Split(",") ?? Array.Empty<string>();
        var allowedMethods = _configuration["Cors:AllowedMethods"]?.Split(",") ?? new[] { "GET","POST","PUT","DELETE","PATCH" };
        var allowedHeaders = _configuration["Cors:AllowedHeaders"]?.Split(",") ?? new[] { "Content-Type","Authorization" };

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods(allowedMethods)
                      .WithHeaders(allowedHeaders);
            });
        });

        /******* [10] Controllers ********/
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        services.AddEndpointsApiExplorer();

        /******* [11] Swagger ********/
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API");
            });
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
