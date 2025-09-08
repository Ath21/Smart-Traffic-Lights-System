using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TrafficLightCacheData;
using TrafficLightCoordinatorData;
using TrafficLightCoordinatorStore.Business.Coordination;
using TrafficLightCoordinatorStore.Consumers;
using TrafficLightCoordinatorStore.Middleware;
using TrafficLightCoordinatorStore.Publishers.Logs;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficLightCoordinatorStore.Repositories.Intersections;
using TrafficLightCoordinatorStore.Repositories.Light;
using TrafficLightCoordinatorStore.Repositories.TrafficConfig;

namespace TrafficLightCoordinatorStore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        /******* [1] ORM / PostgreSQL ********/
        services.AddDbContext<TrafficLightCoordinatorDbContext>();

        /******* [2] Redis (TrafficLight Cache) ********/
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

        /******* [3] Repositories ********/
        services.AddScoped<ITrafficConfigurationRepository, TrafficConfigurationRepository>();
        services.AddScoped<IIntersectionRepository, IntersectionRepository>();
        services.AddScoped<ITrafficLightRepository, TrafficLightRepository>();

        /******* [4] Services ********/
        services.AddScoped<ICoordinatorService, CoordinatorService>();

        /******* [5] AutoMapper ********/
        services.AddAutoMapper(typeof(TrafficLightCoordinatorStoreProfile));

        /******* [6] Publishers ********/
        services.AddScoped<ILightUpdatePublisher, LightUpdatePublisher>();
        services.AddScoped<ITrafficLogPublisher, TrafficLogPublisher>();

        /******* [7] Consumers ********/
        services.AddScoped<TrafficCongestionAlertConsumer>();
        services.AddScoped<PriorityMessageConsumer>();

        /******* [8] MassTransit Setup *******/
        services.AddTrafficCoordinatorMassTransit(_configuration);

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
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        services.AddEndpointsApiExplorer();

        /******* [12] Swagger ********/
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Light Coordinator API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Light Coordinator API");
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
