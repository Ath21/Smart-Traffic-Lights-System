using System;
using IntersectionControlStore.Middleware;
using IntersectionControlStore.Publishers.LightPub;
using IntersectionControlStore.Publishers.LogPub;
using IntersectionControlStore.Publishers.PriorityPub;
using IntersectionControlStore.Consumers; // assuming your consumers are in this namespace
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using TrafficLightControlService.Consumers;

using IntersectionControlStore.Business;
using IntersectionControllerData;
using IntersectionControllerStore.Repository.Intersect;
using IntersectionControllerStore.Repository.Light;
using IntersectionControllerStore.Repository.Config;
using IntersectionControllerStore.Repository;
using IntersectionControllerStore;
using IntersectionControllerStore.Business.TrafficConfig;
using IntersectionControllerStore.Business.TrafficLight;
using IntersectionControllerStore.Business.Intersection;
using IntersectionControllerStore.Consumers;

namespace IntersectionControlStore
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

            /******* [2] Repositories ********/
            
            services.AddScoped(typeof(IRedisRepository), typeof(RedisRepository));
            services.AddScoped(typeof(ITrafficConfigurationRepository), typeof(TrafficConfigurationRepository));
            services.AddScoped(typeof(ITrafficLightRepository), typeof(TrafficLightRepository));
            services.AddScoped(typeof(IIntersectionRepository), typeof(IntersectionRepository));

            /******* [3] Services ********/
            
            services.AddScoped(typeof(ITrafficConfigurationService), typeof(TrafficConfigurationService));
            services.AddScoped(typeof(ITrafficLightService), typeof(TrafficLightService));
            services.AddScoped(typeof(IIntersectionService), typeof(IntersectionService));

            /******* [4] AutoMapper ********/

            services.AddAutoMapper(typeof(IntersectionControllerStoreProfile));

            /******* [5] Publishers ********/
            
            services.AddScoped(typeof(IPriorityPublisher), typeof(PriorityPublisher));
            services.AddScoped(typeof(ITrafficLightControlPublisher), typeof(TrafficLightControlPublisher));
            services.AddScoped(typeof(ITrafficLogPublisher), typeof(TrafficLogPublisher));

            /******* [6] Consumers ********/

            services.AddScoped(typeof(TrafficLightUpdateConsumer));
            services.AddScoped(typeof(SensorDataConsumer));
            
            /******* [7] MassTransit ********/

            services.AddIntersectionControllerMassTransit(_configuration);

            /******* [8] Controllers ********/

            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            /******* [9] Exception Middleware ********/
            // Middleware will be added in Configure method

            /******* [10] Swagger ********/
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Intersection Controller API", Version = "v2.0" });
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

            /******* [11] Authentication & Authorization ********/
            // Configure your authentication here
            // services.AddAuthentication(...);
            // services.AddAuthorization(...);
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intersection Controller API");
                });
            }

            app.UseHttpsRedirection();

            // Exception Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // Authentication & Authorization Middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
