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
            x.AddConsumer<UserFailoverLogConsumer>();
            x.AddConsumer<TrafficAuditLogConsumer>();
            x.AddConsumer<TrafficErrorLogConsumer>();
            x.AddConsumer<TrafficFailoverLogConsumer>();
            x.AddConsumer<SensorAuditLogConsumer>();
            x.AddConsumer<SensorErrorLogConsumer>();
            x.AddConsumer<SensorFailoverLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                var logsExchange = rabbit["Exchanges:Log"];

                // Queues
                var userAuditQueue     = rabbit["Queues:UserAudit"];
                var userErrorQueue     = rabbit["Queues:UserError"];
                var userFailoverQueue  = rabbit["Queues:UserFailover"];
                var trafficAuditQueue  = rabbit["Queues:TrafficAudit"];
                var trafficErrorQueue  = rabbit["Queues:TrafficError"];
                var trafficFailoverQueue = rabbit["Queues:TrafficFailover"];
                var sensorAuditQueue   = rabbit["Queues:SensorAudit"];
                var sensorErrorQueue   = rabbit["Queues:SensorError"];
                var sensorFailoverQueue = rabbit["Queues:SensorFailover"];

                // Routing key sets from config
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

                var userFailoverKeys = new[]
                {
                    rabbit["RoutingKeys:User:UserServiceFailover"],
                    rabbit["RoutingKeys:User:NotificationServiceFailover"]
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

                var trafficFailoverKeys = new[]
                {
                    rabbit["RoutingKeys:Traffic:AnalyticsFailover"],
                    rabbit["RoutingKeys:Traffic:CoordinatorFailover"],
                    rabbit["RoutingKeys:Traffic:IntersectionFailover"],
                    rabbit["RoutingKeys:Traffic:LightControllerFailover"]
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

                var sensorFailoverKeys = new[]
                {
                    rabbit["RoutingKeys:Sensor:SensorFailover"],
                    rabbit["RoutingKeys:Sensor:DetectionFailover"]
                };

                // ================= USER AUDIT =================
                cfg.ReceiveEndpoint(userAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in userAuditKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<UserAuditLogConsumer>(context);
                });

                // ================= USER ERROR =================
                cfg.ReceiveEndpoint(userErrorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in userErrorKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<UserErrorLogConsumer>(context);
                });

                // ================= USER FAILOVER =================
                cfg.ReceiveEndpoint(userFailoverQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in userFailoverKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<UserFailoverLogConsumer>(context);
                });

                // ================= TRAFFIC AUDIT =================
                cfg.ReceiveEndpoint(trafficAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in trafficAuditKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<TrafficAuditLogConsumer>(context);
                });

                // ================= TRAFFIC ERROR =================
                cfg.ReceiveEndpoint(trafficErrorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in trafficErrorKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<TrafficErrorLogConsumer>(context);
                });

                // ================= TRAFFIC FAILOVER =================
                cfg.ReceiveEndpoint(trafficFailoverQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in trafficFailoverKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<TrafficFailoverLogConsumer>(context);
                });

                // ================= SENSOR AUDIT =================
                cfg.ReceiveEndpoint(sensorAuditQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in sensorAuditKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<SensorAuditLogConsumer>(context);
                });

                // ================= SENSOR ERROR =================
                cfg.ReceiveEndpoint(sensorErrorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in sensorErrorKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<SensorErrorLogConsumer>(context);
                });

                // ================= SENSOR FAILOVER =================
                cfg.ReceiveEndpoint(sensorFailoverQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in sensorFailoverKeys)
                        e.Bind(logsExchange, s => { s.RoutingKey = key; s.ExchangeType = ExchangeType.Topic; });

                    e.ConfigureConsumer<SensorFailoverLogConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
