using MassTransit;
using RabbitMQ.Client;
using LogStore.Consumers.User;
using LogStore.Consumers.Traffic;
using LogStore.Consumers.Sensor;

namespace LogStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddLogServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Consumers
            x.AddConsumer<UserAuditLogConsumer>();
            x.AddConsumer<UserErrorLogConsumer>();
            x.AddConsumer<TrafficAuditLogConsumer>();
            x.AddConsumer<TrafficErrorLogConsumer>();
            x.AddConsumer<SensorAuditLogConsumer>();
            x.AddConsumer<SensorErrorLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var logsExchange = rabbit["Exchanges:Log"];

                // Queues
                var userAuditQueue    = rabbit["Queues:UserAudit"];
                var userErrorQueue    = rabbit["Queues:UserError"];
                var trafficAuditQueue = rabbit["Queues:TrafficAudit"];
                var trafficErrorQueue = rabbit["Queues:TrafficError"];
                var sensorAuditQueue  = rabbit["Queues:SensorAudit"];
                var sensorErrorQueue  = rabbit["Queues:SensorError"];

                // Routing keys
                var userAuditKeys = new[]
                {
                    rabbit["RoutingKeys:User:UserServiceAudit"],
                    rabbit["RoutingKeys:User:NotificationServiceAudit"]
                };

                var userErrorKeys = new[]
                {
                    rabbit["RoutingKeys:User:UserServiceError"],
                    rabbit["RoutingKeys:User:NotificationServiceError"]
                };

                var trafficAuditKeys = new[]
                {
                    rabbit["RoutingKeys:Traffic:AnalyticsAudit"],
                    rabbit["RoutingKeys:Traffic:CoordinatorAudit"],
                    rabbit["RoutingKeys:Traffic:IntersectionAudit"],
                    rabbit["RoutingKeys:Traffic:LightControllerAudit"]
                };

                var trafficErrorKeys = new[]
                {
                    rabbit["RoutingKeys:Traffic:AnalyticsError"],
                    rabbit["RoutingKeys:Traffic:CoordinatorError"],
                    rabbit["RoutingKeys:Traffic:IntersectionError"],
                    rabbit["RoutingKeys:Traffic:LightControllerError"]
                };

                var sensorAuditKeys = new[]
                {
                    rabbit["RoutingKeys:Sensor:SensorAudit"],
                    rabbit["RoutingKeys:Sensor:DetectionAudit"]
                };

                var sensorErrorKeys = new[]
                {
                    rabbit["RoutingKeys:Sensor:SensorError"],
                    rabbit["RoutingKeys:Sensor:DetectionError"]
                };

                // ================= USER AUDIT QUEUE =================
                cfg.ReceiveEndpoint(userAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in userAuditKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<UserAuditLogConsumer>(context);
                });

                // ================= USER ERROR QUEUE =================
                cfg.ReceiveEndpoint(userErrorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in userErrorKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<UserErrorLogConsumer>(context);
                });

                // ================= TRAFFIC AUDIT QUEUE =================
                cfg.ReceiveEndpoint(trafficAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in trafficAuditKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<TrafficAuditLogConsumer>(context);
                });

                // ================= TRAFFIC ERROR QUEUE =================
                cfg.ReceiveEndpoint(trafficErrorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in trafficErrorKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<TrafficErrorLogConsumer>(context);
                });

                // ================= SENSOR AUDIT QUEUE =================
                cfg.ReceiveEndpoint(sensorAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in sensorAuditKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<SensorAuditLogConsumer>(context);
                });

                // ================= SENSOR ERROR QUEUE =================
                cfg.ReceiveEndpoint(sensorErrorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in sensorErrorKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<SensorErrorLogConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
