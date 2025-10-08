using System.Text;
using TrafficLightCacheData.Repositories;
using TrafficLightCacheData.Settings;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Consumers;
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
        /******* [1] Redis Config (TrafficLight Cache) ********/
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

        /******* [2] Repositories ********/
        /******* [2.1] TrafficLightCacheDB Repositories ********/
        services.AddScoped(typeof(ITrafficLightCacheRepository), typeof(TrafficLightCacheRepository));

        /******* [3] Services ********/
        services.AddScoped(typeof(ITrafficLightControlService), typeof(TrafficLightControlService));

        /******* [4] Failover Service ********/
        services.AddScoped(typeof(IFailoverService), typeof(FailoverService));

        /******* [6] Automapper ********/
        services.AddAutoMapper(typeof(TrafficLightControlStoreProfile));

        /******* [6] Publishers ********/
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        /******* [7] Consumers ********/
        services.AddScoped(typeof(TrafficLightControlConsumer));

        /******* [8] MassTransit ********/
        services.AddTrafficLightControlMassTransit(_configuration);

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
            .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

        /******* [12] Swagger ********/
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
