using LogMessages;
using MassTransit;
using RabbitMQ.Client;
using UserMessages;
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
            x.AddConsumer<TrafficSummaryConsumer>();
            x.AddConsumer<TrafficIncidentConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitmq = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitmq["Host"], "/", h =>
                {
                    h.Username(rabbitmq["Username"]);
                    h.Password(rabbitmq["Password"]);
                });

                // =========================
                // 🔹 LOGS (Publish)
                // =========================
                cfg.Message<AuditLogMessage>(e => e.SetEntityName("USER.LOGS.EXCHANGE"));
                cfg.Publish<AuditLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<ErrorLogMessage>(e => e.SetEntityName("USER.LOGS.EXCHANGE"));
                cfg.Publish<ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // 🔹 USER NOTIFICATIONS (Publish + Consume)
                // =========================
                cfg.Message<UserNotificationRequest>(e => e.SetEntityName("USER.EXCHANGE"));
                cfg.Publish<UserNotificationRequest>(e => e.ExchangeType = ExchangeType.Direct);

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
                // 🔹 TRAFFIC EVENTS (Consume)
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
