using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserData;
using UserStore.Business.Password;
using UserStore.Business.Token;
using UserStore.Business.Usr;
using UserStore.Middleware;
using UserStore.Repository.Audit;
using UserStore.Repository.Ses;
using UserStore.Repository.Usr;
using RabbitMQ.Client;
using UserStore.Publishers;
using UserMessages;
using UserStore.Publishers.Logs;
using UserStore.Publishers.Notifications;
using UserStore.Publishers.Traffic;
using UserStore.Consumers;


namespace UserStore;

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

        services.AddDbContext<UserDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(IAuditLogRepository), typeof(AuditLogRepository));
        services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
        services.AddScoped(typeof(ISessionRepository), typeof(SessionRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(IPasswordHasher), typeof(PasswordHasher));
        services.AddScoped(typeof(ITokenService), typeof(TokenService));
        services.AddScoped(typeof(IUsrService), typeof(UsrService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(UserStoreProfile));

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

        /******* [6] Publishers ********/

        services.AddScoped(typeof(IUserLogPublisher), typeof(UserLogPublisher));
        services.AddScoped(typeof(IUserNotificationPublisher), typeof(UserNotificationPublisher));
        services.AddScoped(typeof(ITrafficPublisher), typeof(TrafficPublisher));

        /******* [7] Consumers ********/

        services.AddScoped<UserNotificationAlertConsumer>();
        services.AddScoped<PublicNoticeConsumer>();
        services.AddScoped<TrafficCongestionConsumer>();
        services.AddScoped<TrafficSummaryConsumer>();
        services.AddScoped<TrafficIncidentConsumer>();

        /******* [8] MassTransit ********/

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TrafficCongestionConsumer>();
            x.AddConsumer<TrafficSummaryConsumer>();
            x.AddConsumer<TrafficIncidentConsumer>();
            x.AddConsumer<UserNotificationAlertConsumer>();
            x.AddConsumer<PublicNoticeConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmqSettings = _configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitmqSettings["Host"], "/", h =>
                {
                    h.Username(rabbitmqSettings["Username"]);
                    h.Password(rabbitmqSettings["Password"]);
                });

                // =========================
                // 🔹 LOGS (Publish)
                // =========================
                cfg.Message<LogAudit>(e => e.SetEntityName(rabbitmqSettings["Exchanges:Logs"]));
                cfg.Publish<LogAudit>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<LogError>(e => e.SetEntityName(rabbitmqSettings["Exchanges:Logs"]));
                cfg.Publish<LogError>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // 🔹 USER NOTIFICATIONS (Publish)
                // =========================
                cfg.Message<NotificationRequest>(e => e.SetEntityName(rabbitmqSettings["Exchanges:User"]));
                cfg.Publish<NotificationRequest>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<NotificationAlert>(e => e.SetEntityName(rabbitmqSettings["Exchanges:User"]));
                cfg.Publish<NotificationAlert>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<PublicNotice>(e => e.SetEntityName(rabbitmqSettings["Exchanges:User"]));
                cfg.Publish<PublicNotice>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // 🔹 TRAFFIC EVENTS (Consume)
                // =========================
                cfg.ReceiveEndpoint(rabbitmqSettings["Queues:Traffic"], e =>
                {
                    e.ConfigureConsumeTopology = false; // we bind manually
                    e.Bind(rabbitmqSettings["Exchanges:Traffic"], s =>
                    {
                        s.RoutingKey = rabbitmqSettings["RoutingKeys:TrafficCongestion"];
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(rabbitmqSettings["Exchanges:Traffic"], s =>
                    {
                        s.RoutingKey = rabbitmqSettings["RoutingKeys:TrafficSummary"];
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(rabbitmqSettings["Exchanges:Traffic"], s =>
                    {
                        s.RoutingKey = rabbitmqSettings["RoutingKeys:TrafficIncident"];
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficCongestionConsumer>(context);
                    e.ConfigureConsumer<TrafficSummaryConsumer>(context);
                    e.ConfigureConsumer<TrafficIncidentConsumer>(context);
                });

                // =========================
                // 🔹 USER ALERTS & PUBLIC NOTICES (Consume)
                // =========================
                cfg.ReceiveEndpoint(rabbitmqSettings["Queues:User"], e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind(rabbitmqSettings["Exchanges:User"], s =>
                    {
                        s.RoutingKey = rabbitmqSettings["RoutingKeys:NotificationAlert"];
                        s.ExchangeType = ExchangeType.Direct;
                    });
                    e.Bind(rabbitmqSettings["Exchanges:User"], s =>
                    {
                        s.RoutingKey = rabbitmqSettings["RoutingKeys:NotificationPublic"];
                        s.ExchangeType = ExchangeType.Direct;
                    });

                    e.ConfigureConsumer<UserAlertConsumer>(context);
                    e.ConfigureConsumer<PublicNoticeConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API");
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
