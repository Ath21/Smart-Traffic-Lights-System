using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TrafficLightCacheData;
using TrafficLightCacheData.Repositories;
using TrafficLightCacheData.Settings;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Consumers;
using TrafficLightControllerStore.Domain;
using TrafficLightControllerStore.Failover;
using TrafficLightControllerStore.Middleware;
using TrafficLightControllerStore.Publishers.Logs;

namespace TrafficLightControllerStore;

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
        // Traffic Light Context
        // ===============================
        services.AddSingleton(sp =>
        {
            var id = int.Parse(_configuration["TRAFFIC_LIGHT:ID"] ?? throw new InvalidOperationException("Traffic Light Id missing"));
            var name = _configuration["TRAFFIC_LIGHT:NAME"] ?? "Unknown";
            return new TrafficLightContext(id, name);
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
        // Business Layer (Services)
        // ===============================
        services.AddScoped(typeof(ITrafficLightControlService), typeof(TrafficLightControlService));
        services.AddScoped(typeof(IFailoverService), typeof(FailoverService));

        // ===============================
        // AutoMapper (object-object mapping)
        // ===============================
        services.AddAutoMapper(typeof(TrafficLightControlStoreProfile));

        // ===============================
        // Message Layer (MassTransit with RabbitMQ)
        // ===============================
        // Publishers
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        // Consumers
        services.AddScoped<TrafficLightControlConsumer>();

        // MassTransit Setup
        services.AddTrafficLightControlMassTransit(_configuration);

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
        var allowedMethods = _configuration["Cors:AllowedMethods"]?.Split(",") ?? new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
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
        var trafficLightName = _configuration["TRAFFIC_LIGHT:NAME"] ?? "Unknown Traffic Light";

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"Traffic Light Controller – {trafficLightName} ({intersectionName})",
                Version = "v3.0",
                Description = $"Manages and monitors the state of traffic light '{trafficLightName}' at intersection '{intersectionName}'."
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
        var trafficLightName = _configuration["TRAFFIC_LIGHT:NAME"] ?? "Unknown Traffic Light";

        // ===============================
        // Swagger UI
        // ===============================
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Traffic Light Controller – {trafficLightName} ({intersectionName})");
                c.DocumentTitle = $"Traffic Light Controller – {trafficLightName} ({intersectionName})";
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
