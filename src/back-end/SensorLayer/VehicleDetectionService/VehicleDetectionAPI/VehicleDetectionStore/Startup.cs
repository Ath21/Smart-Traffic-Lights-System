using DetectionData;
using LogMessages;
using MassTransit;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using SensorMessages;
using VehicleDetectionService.Middleware;
using VehicleDetectionService.Publishers;
using VehicleDetectionStore.Business;
using VehicleDetectionStore.Publishers;
using VehicleDetectionStore.Repositories;
using VehicleDetectionStore.Workers;

namespace VehicleDetectionStore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        /******* [1] InfluxDB Context ********/

        var influxSettings = _configuration.GetSection("DetectionDb").Get<InfluxDbSettings>();
        services.AddSingleton(influxSettings);
        services.AddSingleton<DetectionDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(IVehicleDetectionRepository), typeof(VehicleDetectionRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(IVehicleDetectService), typeof(VehicleDetectService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(VehicleDetectionStoreProfile));

        /******* [5] Publishers ********/

        services.AddScoped(typeof(IVehicleDetectionPublisher), typeof(VehicleDetectionPublisher));

        /******* [6] MassTransit ********/

        services.AddVehicleDetectionMassTransit(_configuration);

        
        /******* [7] Workers ********/

        services.AddHostedService<VehicleSensor>();

        /******* [8] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vehicle Detection Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicle Detection Service API");
            });
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
