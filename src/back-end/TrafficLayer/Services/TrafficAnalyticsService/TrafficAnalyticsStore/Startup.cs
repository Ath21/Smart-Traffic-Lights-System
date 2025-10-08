using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Repositories.Summary;
using TrafficAnalyticsData.Repositories.Alerts;
using TrafficAnalyticsData.Settings;
using TrafficAnalyticsStore.Middleware;
using TrafficAnalyticsStore.Publishers.Congestion;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficAnalyticsStore.Publishers.Logs;
using TrafficAnalyticsStore.Consumers;
using TrafficAnalyticsStore.Business;
using DetectionCacheData;
using DetectionCacheData.Repositories;
using DetectionCacheData.Settings;

namespace TrafficAnalyticsStore;

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
        // Data Layer (PostgreSQL - TrafficAnalyticsDB)
        // ===============================
        // Db Context
        services.Configure<TrafficAnalyticsDbSettings>(options =>
        {
            options.ConnectionString = _configuration["Postgres:ConnectionString"];
        });
        services.AddDbContext<TrafficAnalyticsDbContext>(options =>
            options.UseNpgsql(_configuration["Postgres:ConnectionString"]));

        // Repositories
        services.AddScoped(typeof(IDailySummaryRepository), typeof(DailySummaryRepository));
        services.AddScoped(typeof(IAlertRepository), typeof(AlertRepository));

        // ===============================
        // Data Layer (Redis - DetectionCacheDB)
        // ===============================
        // Db Context 
        services.Configure<DetectionCacheDbSettings>(options =>
        {
            options.Host = _configuration["Redis:Host"];
            options.Port = int.Parse(_configuration["Redis:Port"]);
            options.Password = _configuration["Redis:Password"];
            options.Database = int.Parse(_configuration["Redis:Database"]);
            options.KeyPrefix = new KeyPrefixSettings
            {
                VehicleCount = _configuration["Redis:KeyPrefix:VehicleCount"],
                PedestrianCount = _configuration["Redis:KeyPrefix:PedestrianCount"],
                CyclistCount = _configuration["Redis:KeyPrefix:CyclistCount"],
                PublicTransportDetections = _configuration["Redis:KeyPrefix:PublicTransportDetections"],
                EmergencyVehicleDetections = _configuration["Redis:KeyPrefix:EmergencyVehicleDetections"],
                IncidentDetections = _configuration["Redis:KeyPrefix:IncidentDetections"]
            };
        });
        services.AddSingleton<DetectionCacheDbContext>();

        // Repositories
        services.AddScoped(typeof(IDetectionCacheRepository), typeof(DetectionCacheRepository));

        // ===============================
        // Business Layer (Services)
        // ===============================
        services.AddScoped(typeof(ITrafficAnalyticsService), typeof(TrafficAnalyticsService));

        // ===============================
        // AutoMapper (object-object mapping)
        // ===============================
        services.AddAutoMapper(typeof(TrafficAnalyticsStoreProfile));

        // ===============================
        // Message Layer (MassTransit with RabbitMQ)
        // ===============================
        // Publishers
        services.AddScoped(typeof(ITrafficCongestionPublisher), typeof(TrafficCongestionPublisher));
        services.AddScoped(typeof(ITrafficIncidentPublisher), typeof(TrafficIncidentPublisher));
        services.AddScoped(typeof(ITrafficSummaryPublisher), typeof(TrafficSummaryPublisher));
        services.AddScoped(typeof(IAnalyticsLogPublisher), typeof(AnalyticsLogPublisher));

        // Consumers
        services.AddScoped<VehicleCountConsumer>();
        services.AddScoped<EmergencyVehicleDetectionConsumer>();
        services.AddScoped<PublicTransportDetectionConsumer>();
        services.AddScoped<PedestrianCountConsumer>();
        services.AddScoped<CyclistCountConsumer>();
        services.AddScoped<IncidentDetectionConsumer>();

        // MassTransit Setup
        services.AddTrafficAnalyticsMassTransit(_configuration);

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
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        // ===============================
        // Swagger (API Documentation)
        // ===============================
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Traffic Analytics Service",
                Version = "v3.0",
                Description = "Aggregates, analyzes, and publishes traffic statistics and congestion data across all intersections."
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Analytics Service");
                c.DocumentTitle = "Traffic Analytics Service";
            });
        }

        // ===============================
        // Core Middleware
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
