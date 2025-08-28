using MassTransit;
using RabbitMQ.Client;
using NotificationStore.Consumers;

namespace NotificationStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddNotificationServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Consumers
            x.AddConsumer<NotificationRequestConsumer>();
            x.AddConsumer<TrafficCongestionConsumer>();
            x.AddConsumer<TrafficIncidentConsumer>();
            x.AddConsumer<TrafficSummaryConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var logsExchange    = rabbit["Exchanges:Logs"];
                var userExchange    = rabbit["Exchanges:User"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var userQueue    = rabbit["Queues:User"];
                var trafficQueue = rabbit["Queues:Traffic"];

                // Routing keys
                var auditKey        = rabbit["RoutingKeys:Audit"];
                var errorKey        = rabbit["RoutingKeys:Error"];
                var notifRequestKey = rabbit["RoutingKeys:NotificationRequest"];
                var notifAlertKey   = rabbit["RoutingKeys:NotificationAlert"];
                var notifPublicKey  = rabbit["RoutingKeys:NotificationPublic"];

                var trafficCongKey      = rabbit["RoutingKeys:TrafficCongestion"];
                var trafficSummaryKey   = rabbit["RoutingKeys:TrafficSummary"];
                var trafficIncidentKey  = rabbit["RoutingKeys:TrafficIncident"];

                // =========================
                // LOGS
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // USER NOTIFICATION REQUESTS
                // =========================
                cfg.Message<UserMessages.UserNotificationRequest>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.UserNotificationRequest>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.ReceiveEndpoint(userQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(userExchange, s =>
                    {
                        s.RoutingKey = notifRequestKey; // consume requests
                        s.ExchangeType = ExchangeType.Direct;
                    });

                    e.ConfigureConsumer<NotificationRequestConsumer>(context);
                });

                // =========================
                // TRAFFIC EVENTS
                // =========================
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    // Replace {intersection_id} with * for wildcard binding
                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficCongKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficIncidentKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficSummaryKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficCongestionConsumer>(context);
                    e.ConfigureConsumer<TrafficIncidentConsumer>(context);
                    e.ConfigureConsumer<TrafficSummaryConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
