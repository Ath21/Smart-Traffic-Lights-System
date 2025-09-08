using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
using TrafficLightCacheData.Repositories;
using TrafficLightCacheData.Repositories.Config;
using TrafficLightCacheData.Repositories.Light;
using TrafficLightCacheData.Repositories.Intersect;

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
        /******* [1] Redis Config ********/
        services.Configure<TrafficLightCacheDbSettings>(options =>
        {
            options.Host = _configuration["Redis:TrafficLight:Host"];
            options.Port = int.Parse(_configuration["Redis:TrafficLight:Port"] ?? "6379");
            options.Password = _configuration["Redis:TrafficLight:Password"];
            options.Database = int.Parse(_configuration["Redis:TrafficLight:Database"] ?? "0");

            options.KeyPrefix_State = _configuration["Redis:TrafficLight:KeyPrefix:State"];
            options.KeyPrefix_Duration = _configuration["Redis:TrafficLight:KeyPrefix:Duration"];
            options.KeyPrefix_LastUpdate = _configuration["Redis:TrafficLight:KeyPrefix:LastUpdate"];
            options.KeyPrefix_Priority = _configuration["Redis:TrafficLight:KeyPrefix:Priority"];
            options.KeyPrefix_QueueLength = _configuration["Redis:TrafficLight:KeyPrefix:QueueLength"];
        });

        services.Configure<DetectionCacheDbSettings>(options =>
        {
            options.Host = _configuration["Redis:Detection:Host"];
            options.Port = int.Parse(_configuration["Redis:Detection:Port"] ?? "6379");
            options.Password = _configuration["Redis:Detection:Password"];
            options.Database = int.Parse(_configuration["Redis:Detection:Database"] ?? "0");

            options.KeyPrefix_VehicleCount = _configuration["Redis:Detection:KeyPrefix:VehicleCount"];
            options.KeyPrefix_PedestrianCount = _configuration["Redis:Detection:KeyPrefix:PedestrianCount"];
            options.KeyPrefix_CyclistCount = _configuration["Redis:Detection:KeyPrefix:CyclistCount"];
            options.KeyPrefix_EmergencyDetected = _configuration["Redis:Detection:KeyPrefix:EmergencyDetected"];
            options.KeyPrefix_PublicTransportDetected = _configuration["Redis:Detection:KeyPrefix:PublicTransportDetected"];
            options.KeyPrefix_IncidentDetected = _configuration["Redis:Detection:KeyPrefix:IncidentDetected"];
        });

        services.AddSingleton<TrafficLightCacheDbContext>();

        /******* [2] Repositories ********/
        services.AddScoped<IRedisRepository, RedisRepository>();
        services.AddScoped<ITrafficConfigurationRepository, TrafficConfigurationRepository>();
        services.AddScoped<ITrafficLightRepository, TrafficLightRepository>();
        services.AddScoped<IIntersectionRepository, IntersectionRepository>();

        /******* [3] Services ********/
        services.AddScoped<ITrafficConfigurationService, TrafficConfigurationService>();
        services.AddScoped<ITrafficLightService, TrafficLightService>();
        services.AddScoped<IIntersectionService, IntersectionService>();
        services.AddScoped<IPriorityManager, PriorityManager>();
        services.AddScoped<ICommandLogService, CommandLogService>();
        services.AddScoped<ITrafficLightCoordinatorService, TrafficLightCoordinatorService>();

        /******* [4] AutoMapper ********/
        services.AddAutoMapper(typeof(IntersectionControllerStoreProfile));

        /******* [5] Publishers ********/
        services.AddScoped<IPriorityPublisher, PriorityPublisher>();
        services.AddScoped<ITrafficLightControlPublisher, TrafficLightControlPublisher>();
        services.AddScoped<ITrafficLogPublisher, TrafficLogPublisher>();

        /******* [6] Consumers ********/
        services.AddScoped<TrafficLightUpdateConsumer>();
        services.AddScoped<SensorDataConsumer>();

        /******* [7] MassTransit ********/
        services.AddIntersectionControllerMassTransit(_configuration);

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
        var allowedMethods = _configuration["Cors:AllowedMethods"]?.Split(",") ?? new[] { "GET","POST","PUT","PATCH","DELETE" };
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
            .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

        /******* [11] Swagger ********/
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Intersection Controller API", Version = "v2.0" });
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
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intersection Controller API");
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
