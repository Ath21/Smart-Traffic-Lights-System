
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
using DetectionCacheData.Healthchecks;
using TrafficLightCacheData.Settings;
using TrafficLightCacheData.Healthchecks;
using TrafficLightCacheData.Repositories;
using DetectionCacheData.Repositories;

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


        services.AddHealthChecks()
            .AddCheck<TrafficLightCacheDbHealthCheck>("trafficlightcachedb_ping");

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

        services.AddHealthChecks()
            .AddCheck<DetectionCacheDbHealthCheck>("detectioncachedb_ping");

        /******* [2] Repositories ********/
        /******* [2.1] DetectionCacheDB Repositories ********/
        services.AddScoped(typeof(IDetectionCacheRepository), typeof(DetectionCacheRepository));

        /******* [2.2] TrafficLightCacheDB Repositories ********/
        services.AddScoped(typeof(ITrafficLightCacheRepository), typeof(TrafficLightCacheRepository));

        /******* [3] Services ********/
        services.AddScoped(typeof(ITrafficConfigurationService), typeof(TrafficConfigurationService));
        services.AddScoped(typeof(ITrafficLightService), typeof(TrafficLightService));
        services.AddScoped(typeof(IIntersectionService), typeof(IntersectionService));
        services.AddScoped(typeof(IPriorityManager), typeof(PriorityManager));
        services.AddScoped(typeof(ICommandLogService), typeof(CommandLogService));
        services.AddScoped(typeof(ITrafficLightCoordinatorService), typeof(TrafficLightCoordinatorService));

        /******* [4] Failover ********/
        services.AddScoped(typeof(IFailoverService), typeof(FailoverService));

        /******* [5] AutoMapper ********/
        services.AddAutoMapper(typeof(IntersectionControllerStoreProfile));

        /******* [6] Publishers ********/
        services.AddScoped(typeof(IPriorityPublisher), typeof(PriorityPublisher));
        services.AddScoped(typeof(ITrafficLightControlPublisher), typeof(TrafficLightControlPublisher));
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        /******* [7] Consumers ********/
        services.AddScoped<TrafficLightUpdateConsumer>();
        services.AddScoped<SensorDataConsumer>();

        /******* [8] MassTransit ********/
        services.AddIntersectionControllerMassTransit(_configuration);

        /******* [9] Jwt Config ********/
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

        /******* [10] CORS Policy ********/
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

        /******* [11] Controllers ********/
        services.AddControllers()
            .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

        /******* [12] Swagger ********/
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Intersection Controller Service", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intersection Controller Service");
                c.DocumentTitle = "Intersection Controller Service";
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
