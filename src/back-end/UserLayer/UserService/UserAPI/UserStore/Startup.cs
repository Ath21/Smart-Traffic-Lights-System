using System.Text;
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
using UserStore.Publishers.Logs;
using UserStore.Publishers.Notifications;
using UserStore.Publishers.Traffic;
using UserStore.Consumers.Traffic;
using UserStore.Consumers.Usr;
using UserStore.Business.Traffic;

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
        services.AddScoped(typeof(ITrafficService), typeof(TrafficService));

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

        services.AddUserServiceMassTransit(_configuration);

        /******* [9] CORS Policy ********/

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173") 
                    .AllowAnyHeader()
                    .AllowAnyMethod();

            });
        });

        /******* [10] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });


        /******* [11] Swagger ********/

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

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
