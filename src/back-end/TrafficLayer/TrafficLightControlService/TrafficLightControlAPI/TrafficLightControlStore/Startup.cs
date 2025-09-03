using System;
using IntersectionControllerData;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using TrafficLightControlStore.Business;
using TrafficLightControlStore.Consumers;
using TrafficLightControlStore.Middleware;
using TrafficLightControlStore.Publishers.Light;
using TrafficLightControlStore.Publishers.Logs;
using TrafficLightControlStore.Repository;

namespace TrafficLightControlStore
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

            services.AddMassTransit(x =>
            {
                x.AddConsumer<TrafficLightControlConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(_configuration["RabbitMQ:Username"]);
                        h.Password(_configuration["RabbitMQ:Password"]);
                    });

                    /* ------------------- TRAFFIC CONTROL ------------------- */
                    string trafficControlExchange = _configuration["RabbitMQ:Exchange:TrafficControlExchange"];

                    cfg.Message<TrafficLightControl>(m => m.SetEntityName(trafficControlExchange));
                    cfg.Message<TrafficLightStateUpdate>(m => m.SetEntityName(trafficControlExchange));

                    cfg.Publish<TrafficLightControl>(m => m.ExchangeType = ExchangeType.Topic);
                    cfg.Publish<TrafficLightStateUpdate>(m => m.ExchangeType = ExchangeType.Topic);

                    // Receive control commands
                    cfg.ReceiveEndpoint(_configuration["RabbitMQ:Queue:TrafficLightControlQueue"], e =>
                    {
                        e.Bind(trafficControlExchange, s =>
                        {
                            s.RoutingKey = _configuration["RabbitMQ:RoutingKey:TrafficLightControl"];
                            s.ExchangeType = ExchangeType.Topic;
                        });
                        e.ConfigureConsumer<TrafficLightControlConsumer>(context);
                    });

                    /* ------------------- LOGGING ------------------- */
                    string logExchange = _configuration["RabbitMQ:Exchange:LogStoreExchange"];

                    cfg.Message<AuditLogMessage>(m => m.SetEntityName(logExchange));
                    cfg.Message<ErrorLogMessage>(m => m.SetEntityName(logExchange));

                    cfg.Publish<AuditLogMessage>(m => m.ExchangeType = ExchangeType.Direct);
                    cfg.Publish<ErrorLogMessage>(m => m.ExchangeType = ExchangeType.Direct);
                });
            });

            /******* [5] Controllers ********/
            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            /******* [6] Swagger ********/
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Traffic Light Control API",
                    Version = "v1.0"
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

            /******* [7] Authentication & Authorization ********/
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Light Control API");
                });
            }

            app.UseHttpsRedirection();

            // Exception Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
