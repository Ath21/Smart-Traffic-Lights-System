using System.Text;
using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorData;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Business.Coordination;
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
        /******* [1] ORM ********/

        services.AddDbContext<TrafficLightCoordinatorDbContext>(opt =>
        {
            opt.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.UseNetTopologySuite());
        });

        /******* [2] Repositories ********/

        services.AddScoped(typeof(ITrafficConfigurationRepository), typeof(TrafficConfigurationRepository));
        services.AddScoped(typeof(IIntersectionRepository), typeof(IntersectionRepository));
        services.AddScoped(typeof(ITrafficLightRepository), typeof(TrafficLightRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(IScheduleService), typeof(ScheduleService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(TrafficLightCoordinatorStoreProfile));

        /******* [6] MassTransit ********/

        services.AddScoped(typeof(ILightUpdatePublisher), typeof(LightUpdatePublisher));
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
                });

                // Publisher for LogInfo
                cfg.Message<LogInfo>(e => { e.SetEntityName(rabbitmqSettings["UserLogsExchange"]); }); 
                cfg.Publish<LogInfo>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.Message<LogAudit>(e => { e.SetEntityName(rabbitmqSettings["UserLogsExchange"]); });
                cfg.Publish<LogAudit>(e => { e.ExchangeType = ExchangeType.Direct;});

                cfg.Message<LogError>(e => { e.SetEntityName(rabbitmqSettings["UserLogsExchange"]); });
                cfg.Publish<LogError>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.Message<NotificationRequest>(e => { e.SetEntityName(rabbitmqSettings["UserNotificationsExchange"]); });
                cfg.Publish<NotificationRequest>(e => { e.ExchangeType = ExchangeType.Direct; });

                cfg.ConfigureEndpoints(context);
            });
        });

        /******* [7] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API");
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
