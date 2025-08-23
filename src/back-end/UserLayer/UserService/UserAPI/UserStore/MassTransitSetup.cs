using MassTransit;
using RabbitMQ.Client;
using UserStore.Consumers;

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
                var rabbitmq = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitmq["Host"], "/", h =>
                {
                    h.Username(rabbitmq["Username"]);
                    h.Password(rabbitmq["Password"]);
                });

                // =========================
                // LOGS (Publish only)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName("LOG.EXCHANGE"));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName("LOG.EXCHANGE"));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // USER NOTIFICATIONS
                // =========================
                cfg.Message<UserMessages.UserNotificationRequest>(e => e.SetEntityName("USER.EXCHANGE"));
                cfg.Publish<UserMessages.UserNotificationRequest>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.ReceiveEndpoint("use.user_service.queue", e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind("USER.EXCHANGE", s =>
                    {
                        s.RoutingKey = "user.notification.alert";
                        s.ExchangeType = ExchangeType.Direct;
                    });

                    e.Bind("USER.EXCHANGE", s =>
                    {
                        s.RoutingKey = "notification.event.public_notice";
                        s.ExchangeType = ExchangeType.Direct;
                    });

                    e.ConfigureConsumer<UserNotificationAlertConsumer>(context);
                    e.ConfigureConsumer<PublicNoticeConsumer>(context);
                });

                // =========================
                // TRAFFIC EVENTS
                // =========================
                cfg.ReceiveEndpoint("traffic.user_service.queue", e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind("TRAFFIC.EXCHANGE", s =>
                    {
                        s.RoutingKey = "traffic.analytics.congestion.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind("TRAFFIC.EXCHANGE", s =>
                    {
                        s.RoutingKey = "traffic.analytics.incident.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind("TRAFFIC.EXCHANGE", s =>
                    {
                        s.RoutingKey = "traffic.analytics.summary.*";
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
