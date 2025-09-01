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

                // Centralized log exchange
                var logsExchange = rabbit["Exchanges:Logs"];

                // Queues
                var userQueue    = rabbit["Queues:UserLogs"];
                var trafficQueue = rabbit["Queues:TrafficLogs"];
                var sensorQueue  = rabbit["Queues:SensorLogs"];

                // Routing keys
                var userAuditKey    = rabbit["RoutingKeys:UserAudit"];
                var userErrorKey    = rabbit["RoutingKeys:UserError"];
                var trafficAuditKey = rabbit["RoutingKeys:TrafficAudit"];
                var trafficErrorKey = rabbit["RoutingKeys:TrafficError"];
                var sensorAuditKey  = rabbit["RoutingKeys:SensorAudit"];
                var sensorErrorKey  = rabbit["RoutingKeys:SensorError"];

                // ============================================================
                // PUBLISH DEFINITIONS (LOGS) → LOG.EXCHANGE
                // ============================================================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // ============================================================
                // USER LOG QUEUE (from LOG.EXCHANGE)
                // ============================================================
                cfg.ReceiveEndpoint(userQueue, e =>
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

                    e.ConfigureConsumer<UserAuditLogConsumer>(context);
                    e.ConfigureConsumer<UserErrorLogConsumer>(context);
                });

                // ============================================================
                // TRAFFIC LOG QUEUE (from LOG.EXCHANGE)
                // ============================================================
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = trafficAuditKey.Replace("{service_name}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = trafficErrorKey.Replace("{service_name}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficAuditLogConsumer>(context);
                    e.ConfigureConsumer<TrafficErrorLogConsumer>(context);
                });

                // ============================================================
                // SENSOR LOG QUEUE (from LOG.EXCHANGE)
                // ============================================================
                cfg.ReceiveEndpoint(sensorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = sensorAuditKey.Replace("{service_name}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(logsExchange, s =>
                    {
                        s.RoutingKey = sensorErrorKey.Replace("{service_name}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<SensorAuditLogConsumer>(context);
                    e.ConfigureConsumer<SensorErrorLogConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
