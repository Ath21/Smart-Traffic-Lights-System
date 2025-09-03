using System;
using System.Text;
using IntersectionControllerData;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using TrafficLightControllerStore.Business;
using TrafficLightControllerStore.Consumers;
using TrafficLightControllerStore.Middleware;
using TrafficLightControllerStore.Publishers.Logs;
using TrafficLightControllerStore.Repository;

namespace TrafficLightControllerStore
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            /******* [1] Redis Config ********/

            var redisSettings = new RedisSettings();
            _configuration.GetSection("Redis").Bind(redisSettings);
            services.AddSingleton(redisSettings);

            services.AddSingleton<TrafficLightDbMemoryContext>();

            /******* [2] Repositories ********/

            services.AddScoped<ITrafficLightRepository, TrafficLightRepository>();

            /******* [3] Services ********/

            services.AddScoped(typeof(ITrafficLightControlService), typeof(TrafficLightControlService));

            /******* [4] Automapper ********/

            services.AddAutoMapper(typeof(TrafficLightControlStoreProfile));

            /******* [5] Publishers ********/

            services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

            /******* [6] Consumers ********/

            services.AddScoped<TrafficLightControlConsumer>();

            /******* [7] MassTransit ********/

            services.AddTrafficLightControlMassTransit(_configuration);

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

            /******* [10] Controllers ********/

            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            /******* [11] Swagger ********/

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Traffic Light Controller API",
                    Version = "v2.0"
                });
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
                        Array.Empty<string>()
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Light Controller API");
                });
            }

            app.UseHttpsRedirection();

            // Exception Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
