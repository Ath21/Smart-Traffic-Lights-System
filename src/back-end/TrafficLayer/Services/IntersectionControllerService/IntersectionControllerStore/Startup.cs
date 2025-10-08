using IntersectionControllerStore.Middleware;
using IntersectionControllerStore.Business.TrafficConfig;
using IntersectionControllerStore.Business.TrafficLight;
using IntersectionControllerStore.Business.Intersection;
using IntersectionControllerStore.Business.Priority;
using IntersectionControllerStore.Business.Coordinator;
using IntersectionControllerStore.Business.CommandLog;
using IntersectionControllerStore.Publishers.LightPub;
using IntersectionControllerStore.Publishers.LogPub;
using IntersectionControllerStore.Publishers.PriorityPub;
using IntersectionControllerStore.Consumers;
using TrafficLightCacheData;
using DetectionCacheData;
using IntersectionControllerStore.Failover;
using DetectionCacheData.Settings;
using TrafficLightCacheData.Settings;
using TrafficLightCacheData.Repositories;
using DetectionCacheData.Repositories;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IntersectionControllerStore.Domain;

namespace IntersectionControllerStore;

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
        // Intersection Context
        // ===============================
        services.AddSingleton(sp =>
        {
            var id = int.Parse(_configuration["INTERSECTION:ID"] ?? throw new InvalidOperationException("Intersection Id missing"));
            var name = _configuration["INTERSECTION:NAME"] ?? "Unknown";
            return new IntersectionContext(id, name);
        });

        // ===============================
        // Data Layer (Redis - TrafficLightCacheDB)
        // ===============================
        // Db Context
        services.Configure<TrafficLightCacheDbSettings>(options =>
        {
            options.Host = _configuration["Redis:Host"];
            options.Port = int.Parse(_configuration["Redis:Port"] ?? "6379");
            options.Password = _configuration["Redis:Password"];
            options.Database = int.Parse(_configuration["Redis:Database"] ?? "0");

            options.KeyPrefix = new TrafficLightCacheData.Settings.KeyPrefixSettings
            {
                State = _configuration["Redis:KeyPrefix:State"],
                Duration = _configuration["Redis:KeyPrefix:Duration"],
                LastUpdate = _configuration["Redis:KeyPrefix:LastUpdate"],
                Mode = _configuration["Redis:KeyPrefix:Mode"],
                Priority = _configuration["Redis:KeyPrefix:Priority"],
                FailoverActive = _configuration["Redis:KeyPrefix:FailoverActive"],
                Heartbeat = _configuration["Redis:KeyPrefix:Heartbeat"],
                LastCoordinatorSync = _configuration["Redis:KeyPrefix:LastCoordinatorSync"]
            };
        });
        services.AddSingleton<TrafficLightCacheDbContext>();

        // Repositories
        services.AddScoped(typeof(ITrafficLightCacheRepository), typeof(TrafficLightCacheRepository));

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
            options.KeyPrefix = new DetectionCacheData.Settings.KeyPrefixSettings
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
        services.AddScoped(typeof(ITrafficConfigurationService), typeof(TrafficConfigurationService));
        services.AddScoped(typeof(ITrafficLightService), typeof(TrafficLightService));
        services.AddScoped(typeof(IIntersectionService), typeof(IntersectionService));
        services.AddScoped(typeof(IPriorityManager), typeof(PriorityManager));
        services.AddScoped(typeof(ICommandLogService), typeof(CommandLogService));
        services.AddScoped(typeof(ITrafficLightCoordinatorService), typeof(TrafficLightCoordinatorService));
        services.AddScoped(typeof(IFailoverService), typeof(FailoverService));

        // ===============================
        // AutoMapper (object-object mapping)
        // ===============================
        services.AddAutoMapper(typeof(IntersectionControllerStoreProfile));

        // ===============================
        // Message Layer (MassTransit with RabbitMQ)
        // ===============================
        // Publishers
        services.AddScoped(typeof(IPriorityPublisher), typeof(PriorityPublisher));
        services.AddScoped(typeof(ITrafficLightControlPublisher), typeof(TrafficLightControlPublisher));
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        // Consumers
        services.AddScoped<TrafficLightUpdateConsumer>();
        services.AddScoped<SensorDataConsumer>();

        // MassTransit Setup
        services.AddIntersectionControllerMassTransit(_configuration);

        // ===============================
        // JWT (Authentication & Authorization)
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
        var intersectionName = _configuration["INTERSECTION:NAME"] ?? "Unknown Intersection";
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"Intersection Controller Service – {intersectionName}",
                Version = "v3.0",
                Description = $"Controls and coordinates traffic lights for intersection '{intersectionName}'."
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
        var intersectionName = _configuration["INTERSECTION:NAME"] ?? "Unknown Intersection";

        // ===============================
        // Swagger UI
        // ===============================
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Intersection Controller – {intersectionName}");
                c.DocumentTitle = $"Intersection Controller – {intersectionName}";
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
