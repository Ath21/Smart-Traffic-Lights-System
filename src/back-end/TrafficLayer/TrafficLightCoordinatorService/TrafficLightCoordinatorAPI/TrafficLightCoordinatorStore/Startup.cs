using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrafficLightCoordinatorData;
using TrafficLightCoordinatorData.Entities;
using TrafficLightCoordinatorStore.Business.Coordination;
using TrafficLightCoordinatorStore.Consumers;
using TrafficLightCoordinatorStore.Middleware;
using TrafficLightCoordinatorStore.Publishers.Logs;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficLightCoordinatorStore.Repositories.Intersections;
using TrafficLightCoordinatorStore.Repositories.Light;
using TrafficLightCoordinatorStore.Repositories.TrafficConfig;
using TrafficMessages.Analytics;
using TrafficMessages.Priority;



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
        services.AddScoped<PriorityEmergencyVehicleConsumer>();
        services.AddScoped<PriorityPublicTransportConsumer>();
        services.AddScoped<PriorityPedestrianConsumer>();
        services.AddScoped<PriorityCyclistConsumer>();
        services.AddScoped<TrafficCongestionAlertConsumer>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PriorityEmergencyVehicleConsumer>();
            x.AddConsumer<PriorityPublicTransportConsumer>();
            x.AddConsumer<PriorityPedestrianConsumer>();
            x.AddConsumer<PriorityCyclistConsumer>();
            x.AddConsumer<TrafficCongestionAlertConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var rmq = _configuration.GetSection("RabbitMQ");
                cfg.Host(rmq["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rmq["Username"] ?? "guest");
                    h.Password(rmq["Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint(rmq["Queue:TrafficCoordinationQueue"] ?? "traffic.light.coordination", e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind<PriorityEmergencyVehicle>(b => { b.ExchangeType = "topic"; b.RoutingKey = rmq["RoutingKey:PriorityEmergencyKey"] ?? "*"; });
                    e.Bind<PriorityPublicTransport>(b => { b.ExchangeType = "topic"; b.RoutingKey = rmq["RoutingKey:PriorityPublicKey"] ?? "*"; });
                    e.Bind<PriorityPedestrian>(b => { b.ExchangeType = "topic"; b.RoutingKey = rmq["RoutingKey:PriorityPedestrianKey"] ?? "*"; });
                    e.Bind<PriorityCyclist>(b => { b.ExchangeType = "topic"; b.RoutingKey = rmq["RoutingKey:PriorityCyclistKey"] ?? "*"; });

                    e.Consumer<PriorityEmergencyVehicleConsumer>(ctx);
                    e.Consumer<PriorityPublicTransportConsumer>(ctx);
                    e.Consumer<PriorityPedestrianConsumer>(ctx);
                    e.Consumer<PriorityCyclistConsumer>(ctx);
                });
                
                cfg.ReceiveEndpoint(rmq["Queue:TrafficCoordinationQueue"] ?? "traffic.light.coordination", e =>
                {
                    e.ConfigureConsumeTopology = false;

                    // existing binds...
                    e.Bind<TrafficCongestionAlert>(b =>
                    {
                        b.ExchangeType = "topic";
                        b.RoutingKey = rmq["RoutingKey:TrafficAnalyticsCongestionKey"] ?? "*";
                    });

                    e.Consumer<TrafficCongestionAlertConsumer>(ctx);
                });
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Light Coordinator Service API", Version = "v1.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Light Coordinator Service API");
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
