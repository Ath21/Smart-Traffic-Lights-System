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
                var logsExchange    = rabbit["Exchanges:Logs"];
                var userExchange    = rabbit["Exchanges:User"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var userQueue    = rabbit["Queues:User"];
                var trafficQueue = rabbit["Queues:Traffic"];

                // Routing keys
                var notifAlertKey   = rabbit["RoutingKeys:NotificationAlert"];
                var notifPublicKey  = rabbit["RoutingKeys:NotificationPublic"];
                var trafficCongKey  = rabbit["RoutingKeys:TrafficCongestion"];
                var trafficSumKey   = rabbit["RoutingKeys:TrafficSummary"];
                var trafficIncKey   = rabbit["RoutingKeys:TrafficIncident"];

                // =========================
                // LOGS → publish to LOG.EXCHANGE (topic)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // NOTIFICATION REQUESTS -> publish to USER.EXCHANGE
                // =========================
                cfg.Message<UserMessages.UserNotificationRequest>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.UserNotificationRequest>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // USER NOTIFICATIONS
                // =========================
                cfg.Message<UserMessages.UserNotificationAlert>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.UserNotificationAlert>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<UserMessages.PublicNoticeEvent>(e => e.SetEntityName(userExchange));
                cfg.Publish<UserMessages.PublicNoticeEvent>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC COMMANDS -> publish to TRAFFIC.EXCHANGE
                // =========================
                cfg.Message<TrafficMessages.TrafficLightUpdateMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightUpdateMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficLightControlMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightControlMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC EVENTS
                // =========================
                cfg.Message<TrafficMessages.TrafficCongestionMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficCongestionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficIncidentMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficIncidentMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficSummaryMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficSummaryMessage>(e => e.ExchangeType = ExchangeType.Topic);

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
                // TRAFFIC EVENTS
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
                        s.RoutingKey = trafficIncKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficSumKey.Replace("{intersection_id}", "*");
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
