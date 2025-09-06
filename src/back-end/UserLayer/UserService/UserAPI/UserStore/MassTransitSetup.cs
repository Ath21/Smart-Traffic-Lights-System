using MassTransit;
using RabbitMQ.Client;
using UserStore.Consumers.Traffic;
using UserStore.Consumers.Usr;

namespace UserStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddUserServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Consumers
            x.AddConsumer<UserNotificationAlertConsumer>();
            x.AddConsumer<PublicNoticeConsumer>();
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
                var userQueue    = rabbit["Queues:User:Notifications"];
                var trafficQueue = rabbit["Queues:Traffic:Analytics"];

                // Routing Keys
                var notifAlertKey  = rabbit["RoutingKeys:User:NotificationAlert"];
                var notifPublicKey = rabbit["RoutingKeys:User:NotificationPublic"];
                var trafficCongKey = rabbit["RoutingKeys:Traffic:Congestion"];
                var trafficSumKey  = rabbit["RoutingKeys:Traffic:Summary"];
                var trafficIncKey  = rabbit["RoutingKeys:Traffic:Incident"];

                // =========================
                // LOGS → publish to LOG.EXCHANGE
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // NOTIFICATION REQUESTS -> publish to USER.EXCHANGE
                // =========================
                cfg.Message<UserMessages.UserNotificationRequest>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.UserNotificationRequest>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC COMMANDS -> publish to TRAFFIC.EXCHANGE
                // =========================
                cfg.Message<TrafficMessages.TrafficLightUpdateMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightUpdateMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficLightControlMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightControlMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // USER NOTIFICATIONS (consumers)
                // =========================
                cfg.ReceiveEndpoint(userQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(userExchange, s =>
                    {
                        s.RoutingKey = notifAlertKey;
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(userExchange, s =>
                    {
                        s.RoutingKey = notifPublicKey;
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<UserNotificationAlertConsumer>(context);
                    e.ConfigureConsumer<PublicNoticeConsumer>(context);
                });

                // =========================
                // TRAFFIC ANALYTICS EVENTS (consumers)
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
                        s.RoutingKey = trafficSumKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficIncKey.Replace("{intersection_id}", "*");
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
