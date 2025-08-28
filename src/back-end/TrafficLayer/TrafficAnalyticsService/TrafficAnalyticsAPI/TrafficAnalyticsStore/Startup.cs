using Microsoft.OpenApi.Models;
using TrafficAnalyticsData;
using TrafficAnalyticsStore.Middleware;
using TrafficAnalyticsStore.Repository.Summary;
using TrafficAnalyticsStore.Repository.Alerts;
using TrafficAnalyticsStore.Publishers.Congestion;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficAnalyticsStore.Publishers.Logs;
using TrafficAnalyticsStore.Consumers;
using TrafficAnalyticsStore.Business;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TrafficAnalyticsStore;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        /******* [1] PostgreSQL Config ********/

        services.AddDbContext<TrafficAnalyticsDbContext>();

        /******* [2] Repositories ********/

        services.AddScoped(typeof(IDailySummaryRepository), typeof(DailySummaryRepository));
        services.AddScoped(typeof(IAlertRepository), typeof(AlertRepository));
        
        /******* [3] Services ********/

        services.AddScoped(typeof(ITrafficAnalyticsService), typeof(TrafficAnalyticsService));
        
        /******* [4] AutoMapper ********/

        services.AddAutoMapper(typeof(TrafficAnalyticsStoreProfile));

        /******* [5] Publishers ********/

        services.AddScoped(typeof(ITrafficCongestionPublisher), typeof(TrafficCongestionPublisher));
        services.AddScoped(typeof(ITrafficIncidentPublisher), typeof(TrafficIncidentPublisher));
        services.AddScoped(typeof(ITrafficSummaryPublisher), typeof(TrafficSummaryPublisher));
        services.AddScoped(typeof(IAnalyticsLogPublisher), typeof(AnalyticsLogPublisher));

        /******* [6] Consumers ********/

        services.AddScoped(typeof(EmergencyVehicleConsumer));
        services.AddScoped(typeof(PublicTransportConsumer));
        services.AddScoped(typeof(PedestrianDetectionConsumer));
        services.AddScoped(typeof(CyclistDetectionConsumer));
        services.AddScoped(typeof(IncidentDetectionConsumer));

        /******* [7] MassTransit *******/

        services.AddTrafficAnalyticsMassTransit(_configuration);

        /******* [8] Jwt Config ********/

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

        /******* [9] CORS Policy ********/

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")   // Vue dev server
                    .AllowAnyHeader()
                    .AllowAnyMethod();

            });
        });

        /******* [8] Controllers ********/

        services.AddControllers()
            .AddJsonOptions(
                options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddEndpointsApiExplorer();

        /******* [8] Swagger ********/

        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Traffic Analytics API", Version = "v2.0" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Analytics API");
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