using LogData;
using LogStore.Business;
using LogStore.Consumers.User;
using LogStore.Middleware;
using Microsoft.OpenApi.Models;
using LogData.Repositories.Audit;
using LogData.Repositories.Error;
using LogStore.Consumers.Traffic;
using LogStore.Consumers.Sensor;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LogData.Repositories.Failover;

namespace LogStore;

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
        // Data Layer (MongoDB - LogDB)
        // ===============================
        // Db Context
        services.Configure<LogDbSettings>(options =>
        {
            options.ConnectionString = _configuration["Mongo:ConnectionString"];
            options.Database = _configuration["Mongo:Database"];
            options.Collections = new CollectionsSettings
            {
                AuditLogs = _configuration["Mongo:Collections:AuditLogs"],
                ErrorLogs = _configuration["Mongo:Collections:ErrorLogs"],
                FailoverLogs = _configuration["Mongo:Collections:FailoverLogs"]
            };
        });
        services.AddSingleton<LogDbContext>();

        // Repositories
        services.AddScoped(typeof(IAuditLogRepository), typeof(AuditLogRepository));
        services.AddScoped(typeof(IErrorLogRepository), typeof(ErrorLogRepository));
        services.AddScoped(typeof(IFailoverLogRepository), typeof(FailoverLogRepository));

        // ===============================
        // Business Layer (Services)
        // ===============================
        services.AddScoped(typeof(ILogService), typeof(LogService));

        // ===============================
        // AutoMapper (object-object mapping)
        // ===============================
        services.AddAutoMapper(typeof(LogStoreProfile));

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
        // Message Layer (MassTransit with RabbitMQ)
        // ===============================
        // Consumers
        services.AddScoped<UserErrorLogConsumer>();
        services.AddScoped<UserAuditLogConsumer>();
        services.AddScoped<UserFailoverLogConsumer>();
        services.AddScoped<TrafficErrorLogConsumer>();
        services.AddScoped<TrafficAuditLogConsumer>();
        services.AddScoped<TrafficFailoverLogConsumer>();
        services.AddScoped<SensorErrorLogConsumer>();
        services.AddScoped<SensorAuditLogConsumer>();
        services.AddScoped<SensorFailoverLogConsumer>();

        // MassTransit Setup
        services.AddLogServiceMassTransit(_configuration);

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
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Log Service", Version = "v3.0" });
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
        // ===============================
        // Swagger UI
        // ===============================
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Log Service");
                c.DocumentTitle = "Log Service";
            });
        }

        // ===============================
        // Core Middleware
        // ===============================
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseRouting();

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
