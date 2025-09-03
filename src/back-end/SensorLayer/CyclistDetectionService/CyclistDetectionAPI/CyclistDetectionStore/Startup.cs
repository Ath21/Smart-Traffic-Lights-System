using CyclistDetectionStore.Business;
using CyclistDetectionStore.Middleware;
using CyclistDetectionStore.Publishers;
using CyclistDetectionStore.Repositories;
using CyclistDetectionStore.Workers;
using DetectionData;
using LogMessages;
using MassTransit;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using SensorMessages;

namespace CyclistDetectionStore;

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

        services.AddScoped(typeof(ICyclistDetectionRepository), typeof(CyclistDetectionRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(ICyclistDetectService), typeof(CyclistDetectService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(CyclistDetectionStoreProfile));

        /******* [5] Publishers ********/

        services.AddScoped(typeof(ICyclistDetectionPublisher), typeof(CyclistDetectionPublisher));

        /******* [6] MassTransit ********/

        services.AddCyclistDetectionMassTransit(_configuration);

       
        
        /******* [7] Workers ********/

        services.AddHostedService<CyclistSensor>();

        /******* [8] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cyclist Detection Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cyclist Detection Service API");
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
