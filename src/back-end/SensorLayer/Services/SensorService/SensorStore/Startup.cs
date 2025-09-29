using System.Text;
using DetectionCacheData;
using DetectionCacheData.Repositories;
using DetectionData;
using DetectionData.Repositories.Cyclist;
using DetectionData.Repositories.EmergencyVehicle;
using DetectionData.Repositories.Incident;
using DetectionData.Repositories.Pedestrian;
using DetectionData.Repositories.PublicTransport;
using DetectionData.Repositories.Vehicle;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SensorStore.Business;
using SensorStore.Domain;
using SensorStore.Middleware;
using SensorStore.Publishers;
using SensorStore.Publishers.Count;
using SensorStore.Publishers.Logs;
using SensorStore.Workers;


namespace SensorStore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        /******* Intersection Context ********/
        services.AddSingleton(sp =>
        {
            var id = int.Parse(_configuration["INTERSECTION:ID"] ?? throw new InvalidOperationException("Intersection Id missing"));
            var name = _configuration["INTERSECTION:NAME"] ?? "Unknown";
            return new IntersectionContext(id, name);
        });

        /******* [1] MongoDB Config ********/
        services.Configure<DetectionDbSettings>(options =>
        {
            options.ConnectionString = _configuration["Mongo:ConnectionString"];
            options.Database = _configuration["Mongo:Database"];
            options.VehicleCollection = _configuration["Mongo:Collections:VehicleCount"];
            options.PedestrianCollection = _configuration["Mongo:Collections:PedestrianCount"];
            options.CyclistCollection = _configuration["Mongo:Collections:CyclistCount"];
            options.EmergencyCollection = _configuration["Mongo:Collections:Emergency"];
            options.PublicTransportCollection = _configuration["Mongo:Collections:PublicTransport"];
            options.IncidentCollection = _configuration["Mongo:Collections:Incident"];
        });
        services.AddSingleton<DetectionDbContext>();

        /******* [2] Redis Config ********/
        services.Configure<DetectionCacheDbSettings>(options =>
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
        services.AddSingleton<DetectionCacheDbContext>();   

        /******* [3] AutoMapper ********/
        services.AddAutoMapper(typeof(SensorStoreProfile));

        /******* [3] Repositories ********/
        /******* [3.1] DetectionDB Repositories ********/
        services.AddScoped(typeof(IVehicleCountRepository), typeof(VehicleCountRepository));
        services.AddScoped(typeof(IPedestrianCountRepository), typeof(PedestrianCountRepository));
        services.AddScoped(typeof(ICyclistCountRepository), typeof(CyclistCountRepository));
        services.AddScoped(typeof(IEmergencyVehicleDetectionRepository), typeof(EmergencyVehicleDetectionRepository));
        services.AddScoped(typeof(IPublicTransportDetectionRepository), typeof(PublicTransportDetectionRepository));
        services.AddScoped(typeof(IIncidentDetectionRepository), typeof(IncidentDetectionRepository));

        /******* [3.2] DetectionCacheDB Repositories ********/
        services.AddScoped(typeof(IDetectionCacheRepository), typeof(DetectionCacheRepository));

        /******* [4] Services ********/
        services.AddScoped(typeof(ISensorCountService), typeof(SensorCountService));

        /******* [5] Workers ********/
        services.AddHostedService<TrafficSensorWorker>();

        /******* [67] Publishers ********/
        services.AddScoped(typeof(ISensorCountPublisher), typeof(SensorCountPublisher));
        services.AddScoped(typeof(ISensorLogPublisher), typeof(SensorLogPublisher));

        /******* [6] MassTransit ********/
        services.AddSensorServiceMassTransit(_configuration);

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
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sensor Service", Version = "v3.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sensor Service");
                c.DocumentTitle = "Sensor Service";
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
