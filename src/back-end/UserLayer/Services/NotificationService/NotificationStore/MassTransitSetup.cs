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
                var logExchange     = rabbit["Exchanges:Log"];
                var userExchange    = rabbit["Exchanges:User"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var userRequestsQueue = rabbit["Queues:User:Requests"];
                var trafficQueue      = rabbit["Queues:Traffic:Analytics"];

                // Routing keys
                var notifRequestKey = rabbit["RoutingKeys:User:NotificationRequest"];
                var notifAlertKey   = rabbit["RoutingKeys:User:NotificationAlert"];
                var notifPublicKey  = rabbit["RoutingKeys:User:NotificationPublic"];
                var trafficCongKey  = rabbit["RoutingKeys:Traffic:Congestion"];
                var trafficSummaryKey  = rabbit["RoutingKeys:Traffic:Summary"];
                var trafficIncidentKey = rabbit["RoutingKeys:Traffic:Incident"];

                var logAuditKey = rabbit["RoutingKeys:Log:Audit"];
                var logErrorKey = rabbit["RoutingKeys:Log:Error"];

                // =========================
                // LOGS → publish to LOG.EXCHANGE
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // USER NOTIFICATION REQUESTS (consume)
                // =========================
                cfg.ReceiveEndpoint(userRequestsQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(userExchange, s =>
                    {
                        s.RoutingKey = notifRequestKey;
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<NotificationRequestConsumer>(context);
                });

                // =========================
                // USER NOTIFICATIONS (publish)
                // =========================
                cfg.Message<UserMessages.UserNotificationAlert>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.UserNotificationAlert>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<UserMessages.PublicNoticeEvent>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.PublicNoticeEvent>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC EVENTS (consume)
                // =========================
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficCongKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficSummaryKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficIncidentKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficCongestionConsumer>(context);
                    e.ConfigureConsumer<TrafficSummaryConsumer>(context);
                    e.ConfigureConsumer<TrafficIncidentConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
