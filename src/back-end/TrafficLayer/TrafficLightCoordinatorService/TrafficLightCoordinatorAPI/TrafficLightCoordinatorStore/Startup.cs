using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        services.AddDbContext<TrafficLightCoordinatorDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(ITrafficConfigurationRepository), typeof(TrafficConfigurationRepository));
        services.AddScoped(typeof(IIntersectionRepository), typeof(IntersectionRepository));
        services.AddScoped(typeof(ITrafficLightRepository), typeof(TrafficLightRepository));

        /******* [3] Services ********/

        services.AddScoped(typeof(ICoordinatorService), typeof(CoordinatorService));

        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(TrafficLightCoordinatorStoreProfile));

        /******* [5] Publishers ********/

        services.AddScoped(typeof(ILightUpdatePublisher), typeof(LightUpdatePublisher));
        services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

        /******* [6] Consumers ********/

        services.AddScoped<TrafficCongestionAlertConsumer>();
        services.AddScoped<PriorityMessageConsumer>();

        /******* [7] MassTransit Setup


        /******* [8] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

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

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")   // Vue dev server
                    .AllowAnyHeader()
                    .AllowAnyMethod();

            });
        });

        /******* [11] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Light Coordinator API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Light Coordinator API");
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
