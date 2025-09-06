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
                var trafficAuditQueue = rabbit["Queues:TrafficAudit"];
                var sensorAuditQueue  = rabbit["Queues:SensorAudit"];
                var userErrorQueue    = rabbit["Queues:UserError"];
                var trafficErrorQueue = rabbit["Queues:TrafficError"];
                var sensorErrorQueue  = rabbit["Queues:SensorError"];

                // Routing keys (wildcards expanded at binding time)
                var userAuditKey    = rabbit["RoutingKeys:User:UserServiceAudit"];
                var userErrorKey    = rabbit["RoutingKeys:User:UserServiceError"];
                var notifAuditKey   = rabbit["RoutingKeys:User:NotificationServiceAudit"];
                var notifErrorKey   = rabbit["RoutingKeys:User:NotificationServiceError"];

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
                    rabbit["RoutingKeys:Sensor:VehicleAudit"],
                    rabbit["RoutingKeys:Sensor:EmergencyAudit"],
                    rabbit["RoutingKeys:Sensor:PublicTransportAudit"],
                    rabbit["RoutingKeys:Sensor:PedestrianAudit"],
                    rabbit["RoutingKeys:Sensor:CyclistAudit"],
                    rabbit["RoutingKeys:Sensor:IncidentAudit"]
                };

                var sensorErrorKeys = new[]
                {
                    rabbit["RoutingKeys:Sensor:VehicleError"],
                    rabbit["RoutingKeys:Sensor:EmergencyError"],
                    rabbit["RoutingKeys:Sensor:PublicTransportError"],
                    rabbit["RoutingKeys:Sensor:PedestrianError"],
                    rabbit["RoutingKeys:Sensor:CyclistError"],
                    rabbit["RoutingKeys:Sensor:IncidentError"]
                };

                // ================= USER LOG QUEUE =================
                cfg.ReceiveEndpoint(userAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = userAuditKey.Replace("{service_name}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = userErrorKey.Replace("{service_name}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = notifAuditKey;
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = notifErrorKey;
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<UserAuditLogConsumer>(context);
                    e.ConfigureConsumer<UserErrorLogConsumer>(context);
                });

                // ================= TRAFFIC LOG QUEUE =================
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

                    foreach (var key in trafficErrorKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<TrafficAuditLogConsumer>(context);
                    e.ConfigureConsumer<TrafficErrorLogConsumer>(context);
                });

                // ================= SENSOR LOG QUEUE =================
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

                    foreach (var key in sensorErrorKeys)
                    {
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<SensorAuditLogConsumer>(context);
                    e.ConfigureConsumer<SensorErrorLogConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
