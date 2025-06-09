/*
 * UserStore.Startup
 *
 * This class is responsible for configuring the services and middleware for the UserStore application.
 * It contains methods for setting up the dependency injection container, configuring authentication,
 * setting up MassTransit for message queuing, and configuring Swagger for API documentation.
 * The ConfigureServices method is used to register services with the dependency injection container.
 * It includes the following sections:
 *      1. ORM: Configures the Entity Framework Core DbContext for database access.
 *      2. Repositories: Registers the repositories for data access.
 *      3. Services: Registers the business logic services for user management.
 *      4. AutoMapper: Configures AutoMapper for object-to-object mapping.
 *      5. Jwt Config: Configures JWT authentication for securing API endpoints.
 *      6. MassTransit: Configures MassTransit for message queuing with RabbitMQ.
 *      7. Controllers: Configures the API controllers and JSON serialization options.
 *      8. Swagger: Configures Swagger for API documentation and testing.
 * The Configure method is used to set up the middleware pipeline for the application.
 * It includes middleware for exception handling, HTTPS redirection, authentication, and authorization.
 * The method also sets up the Swagger UI for API documentation.
 * The Startup class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserData;
using UserStore.Messages;
using UserStore.Business.Password;
using UserStore.Business.Token;
using UserStore.Business.Usr;
using UserStore.Middleware;
using UserStore.Repository.Audit;
using UserStore.Repository.Ses;
using UserStore.Repository.Usr;
using RabbitMQ.Client;
using UserStore.Publishers;


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

        /******* [6] MassTransit ********/

        services.AddScoped(typeof(IUserLogPublisher), typeof(UserLogPublisher));

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

                // Configure message types to use the appropriate exchanges
                /*cfg.Message<LogInfo>(e => e.SetEntityName(rabbitmqSettings["UserLogsExchange"]));
                cfg.Publish<LogInfo>(e => e.ExchangeType = ExchangeType.Direct);
                cfg.Message<LogAudit>(e => e.SetEntityName(rabbitmqSettings["UserLogsExchange"]));
                cfg.Publish<LogAudit>(e => e.ExchangeType = ExchangeType.Direct);
                cfg.Message<LogError>(e => e.SetEntityName(rabbitmqSettings["UserLogsExchange"]));
                cfg.Publish<LogError>(e => e.ExchangeType = ExchangeType.Direct);*/
                //cfg.Message<NotificationRequest>(e => e.SetEntityName(rabbitmqSettings["UserNotificationsExchange"]));
                //cfg.Publish<NotificationRequest>(e => e.ExchangeType = ExchangeType.Direct);
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
