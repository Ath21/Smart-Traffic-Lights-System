using Microsoft.OpenApi.Models;
using TrafficAnalyticsData;
using TrafficAnalyticsStore.Middleware;
using TrafficAnalyticsStore.Publishers.Congestion;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficAnalyticsStore.Publishers.Logs;
using TrafficAnalyticsStore.Consumers;
using TrafficAnalyticsStore.Business;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DetectionCacheData;
using TrafficAnalyticsData.Repositories.Summary;
using TrafficAnalyticsData.Repositories.Alerts;
using TrafficAnalyticsData.Settings;
using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData.Healthchecks;
using DetectionCacheData.Healthchecks;
using DetectionCacheData.Repositories;

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
        /******* [1] PostgreSQL Config ********/
        services.Configure<TrafficAnalyticsDbSettings>(options =>
        {
            options.ConnectionString = _configuration["Postgres:ConnectionString"];
        });

        services.AddDbContext<TrafficAnalyticsDbContext>(options =>
            options.UseNpgsql(_configuration["Postgres:ConnectionString"]));

        services.AddHealthChecks()
            .AddCheck<TrafficAnalyticsDbHealthCheck>("trafficanalyticsdb_ping");

        /******* [2] Redis (Detection Cache) ********/
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

        services.AddHealthChecks()
            .AddCheck<DetectionCacheDbHealthCheck>("detectioncachedb_ping");


        /******* [3] Repositories ********/
        /******* [3.1] TrafficAnalyticsDB Repositories ********/
        services.AddScoped(typeof(IDailySummaryRepository), typeof(DailySummaryRepository));
        services.AddScoped(typeof(IAlertRepository), typeof(AlertRepository));

        /******* [3.2] DetectionCacheDB Repositories ********/
        services.AddScoped(typeof(IDetectionCacheRepository), typeof(DetectionCacheRepository));

        /******* [4] Services ********/
        services.AddScoped(typeof(ITrafficAnalyticsService), typeof(TrafficAnalyticsService));

        /******* [5] AutoMapper ********/
        services.AddAutoMapper(typeof(TrafficAnalyticsStoreProfile));

        /******* [6] Publishers ********/
        services.AddScoped(typeof(ITrafficCongestionPublisher), typeof(TrafficCongestionPublisher));
        services.AddScoped(typeof(ITrafficIncidentPublisher), typeof(TrafficIncidentPublisher));
        services.AddScoped(typeof(ITrafficSummaryPublisher), typeof(TrafficSummaryPublisher));
        services.AddScoped(typeof(IAnalyticsLogPublisher), typeof(AnalyticsLogPublisher));

        /******* [7] Consumers ********/
        services.AddScoped<VehicleCountConsumer>();
        services.AddScoped<EmergencyVehicleDetectionConsumer>();
        services.AddScoped<PublicTransportDetectionConsumer>();
        services.AddScoped<PedestrianCountConsumer>();
        services.AddScoped<CyclistCountConsumer>();
        services.AddScoped<IncidentDetectionConsumer>();

        /******* [8] MassTransit ********/
        services.AddTrafficAnalyticsMassTransit(_configuration);

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

        /******* [11] Controllers ********/
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        services.AddEndpointsApiExplorer();

        /******* [12] Swagger ********/
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Analytics Service", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Analytics Service");
                c.DocumentTitle = "Traffic Analytics Service";
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
