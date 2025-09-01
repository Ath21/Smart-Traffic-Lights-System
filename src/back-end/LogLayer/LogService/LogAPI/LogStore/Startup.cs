using LogData;
using LogStore.Business;
using LogStore.Consumers.User;
using LogStore.Middleware;
using LogStore.Repository;
using MassTransit;
using UserMessages;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using LogStore.Repository.Audit;
using LogStore.Repository.Error;
using LogStore.Consumers.Traffic;
using LogStore.Consumers.Sensor;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        /******* [1] MongoDb Config ********/

        services.Configure<LogDbSettings>(
            _configuration.GetSection("DefaultConnection")
        );
        services.AddSingleton<LogDbContext>();
        
        /******* [2] Repositories ********/

        services.AddScoped(typeof(IAuditLogRepository), typeof(AuditLogRepository));
        services.AddScoped(typeof(IErrorLogRepository), typeof(ErrorLogRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(ILogService), typeof(LogService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(LogStoreProfile));

        /******* [5] Jwt Config ********/

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

        /******* [6] Consumers ********/

        services.AddScoped<UserErrorLogConsumer>();
        services.AddScoped<UserAuditLogConsumer>();
        services.AddScoped<TrafficErrorLogConsumer>();
        services.AddScoped<TrafficAuditLogConsumer>();
        services.AddScoped<SensorErrorLogConsumer>();
        services.AddScoped<SensorAuditLogConsumer>();

        /******* [7] MassTransit ********/

        services.AddLogServiceMassTransit(_configuration);

        /******* [8] CORS Policy ********/

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173") 
                    .AllowAnyHeader()
                    .AllowAnyMethod();

            });
        });

        /******* [9] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [10] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Log API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Log API");
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
