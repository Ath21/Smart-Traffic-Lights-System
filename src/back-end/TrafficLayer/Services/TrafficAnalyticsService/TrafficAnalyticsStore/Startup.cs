using Microsoft.OpenApi.Models;
using TrafficAnalyticsData;
using TrafficAnalyticsStore.Middleware;
using TrafficAnalyticsStore.Repository.Summary;
using TrafficAnalyticsStore.Repository.Alerts;
using TrafficAnalyticsStore.Publishers.Congestion;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficAnalyticsStore.Publishers.Logs;
using TrafficAnalyticsStore.Consumers;
using TrafficAnalyticsStore.Business;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        services.AddDbContext<TrafficAnalyticsDbContext>();

        /******* [2] Redis (Detection Cache) ********/
        services.Configure<RedisSettings>(options =>
        {
            options.Host = _configuration["Redis:Host"];
            options.Port = int.Parse(_configuration["Redis:Port"] ?? "6379");
            options.Password = _configuration["Redis:Password"];
            options.Database = int.Parse(_configuration["Redis:Database"] ?? "0");

            options.KeyPrefix_VehicleCount = _configuration["Redis:KeyPrefix:VehicleCount"];
            options.KeyPrefix_PedestrianCount = _configuration["Redis:KeyPrefix:PedestrianCount"];
            options.KeyPrefix_CyclistCount = _configuration["Redis:KeyPrefix:CyclistCount"];
            options.KeyPrefix_EmergencyDetected = _configuration["Redis:KeyPrefix:EmergencyDetected"];
            options.KeyPrefix_PublicTransportDetected = _configuration["Redis:KeyPrefix:PublicTransportDetected"];
            options.KeyPrefix_IncidentDetected = _configuration["Redis:KeyPrefix:IncidentDetected"];
        });
        services.AddSingleton<RedisCacheContext>();

        /******* [3] Repositories ********/
        services.AddScoped<IDailySummaryRepository, DailySummaryRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();

        /******* [4] Services ********/
        services.AddScoped<ITrafficAnalyticsService, TrafficAnalyticsService>();

        /******* [5] AutoMapper ********/
        services.AddAutoMapper(typeof(TrafficAnalyticsStoreProfile));

        /******* [6] Publishers ********/
        services.AddScoped<ITrafficCongestionPublisher, TrafficCongestionPublisher>();
        services.AddScoped<ITrafficIncidentPublisher, TrafficIncidentPublisher>();
        services.AddScoped<ITrafficSummaryPublisher, TrafficSummaryPublisher>();
        services.AddScoped<IAnalyticsLogPublisher, AnalyticsLogPublisher>();

        /******* [7] Consumers ********/
        services.AddScoped<VehicleCountConsumer>();
        services.AddScoped<EmergencyVehicleConsumer>();
        services.AddScoped<PublicTransportConsumer>();
        services.AddScoped<PedestrianDetectionConsumer>();
        services.AddScoped<CyclistDetectionConsumer>();
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
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Analytics API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Analytics API");
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
