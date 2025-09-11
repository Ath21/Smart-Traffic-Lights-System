using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using TrafficLightCacheData;
using TrafficLightCacheData.Repositories;
using TrafficLightCacheData.Repositories.Config;
using TrafficLightCacheData.Repositories.Intersect;
using TrafficLightCacheData.Repositories.Light;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Consumers;
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
        /******* [1] Redis Config (TrafficLight Cache) ********/
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
        services.AddSingleton<TrafficLightCacheDbContext>();

        /******* [2] Repositories ********/
        /******* [2.1] TrafficLightCacheDB Repositories ********/
        services.AddScoped(typeof(ITrafficLightRepository), typeof(TrafficLightRepository));
        services.AddScoped(typeof(IIntersectRepository), typeof(IntersectRepository));
        services.AddScoped(typeof(IRedisRepository), typeof(RedisRepository));
        services.AddScoped(typeof(ITrafficConfigRepository), typeof(TrafficConfigRepository));

        /******* [3] Services ********/
        services.AddScoped(typeof(ITrafficLightControlService), typeof(TrafficLightControlService));

        /******* [4] Automapper ********/
        services.AddAutoMapper(typeof(TrafficLightControlStoreProfile));

        /******* [5] Publishers ********/
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        /******* [6] Consumers ********/
        services.AddScoped(typeof(TrafficLightControlConsumer));

        /******* [7] MassTransit ********/
        services.AddTrafficLightControlMassTransit(_configuration);

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
            .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

        /******* [11] Swagger ********/
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Light Controller Service", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Light Controller Service");
                c.DocumentTitle = "Traffic Light Controller Service";
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
