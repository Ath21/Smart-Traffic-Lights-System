using MassTransit;
using RabbitMQ.Client;
using Messages.Log;
using Messages.User;
using Messages.Traffic;
using NotificationStore.Consumers;

namespace NotificationStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddNotificationServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // =====================================================
            // Register Consumers
            // =====================================================
            x.AddConsumer<UserNotificationConsumer>();
            x.AddConsumer<TrafficAnalyticsConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                // =====================================================
                // RabbitMQ Connection
                // =====================================================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // =====================================================
                // Exchanges, Queues, and Routing Keys
                // =====================================================
                // Exchanges
                var userExchange = rabbit["Exchanges:User"];
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var logExchange = rabbit["Exchanges:Log"];

                // Queues
                var userQueue = rabbit["Queues:User:Notification"];
                var trafficQueue = rabbit["Queues:Traffic:Analytics"];

                // Routing Keys
                var userRequestKeys = rabbit.GetSection("RoutingKeys:UserNotifications").Get<string[]>() ?? Array.Empty<string>();
                var trafficMetricsKeys = rabbit.GetSection("RoutingKeys:TrafficAnalytics").Get<string[]>() ?? Array.Empty<string>();

                // =====================================================
                // [PUBLISH] USER NOTIFICATIONS
                // =====================================================
                //
                // Topic pattern : user.notification.{type}
                // Example key   : user.notification.alert
                //
                cfg.Message<UserNotificationMessage>(m =>
                {
                    m.SetEntityName(userExchange);
                });
                cfg.Publish<UserNotificationMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================================
                // [PUBLISH] LOGS
                // =====================================================
                //
                // Topic pattern : log.user.notification.{type}
                // Example key   : log.user.notification.audit
                //
                cfg.Message<LogMessage>(m =>
                {
                    m.SetEntityName(logExchange);
                });
                cfg.Publish<LogMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================================
                // [CONSUME] USER NOTIFICATION REQUESTS
                // =====================================================
                //
                // Topic pattern : user.notification.{type}
                // Example key   : user.notification.request
                //
                cfg.ReceiveEndpoint(userQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in userRequestKeys)
                    {
                        e.Bind(userExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<UserNotificationConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // =====================================================
                // [CONSUME] TRAFFIC ANALYTICS METRICS
                // =====================================================
                //
                // Topic pattern : traffic.analytics.{intersection}.{metric}
                // Example key   : traffic.analytics.agiou-spyridonos.congestion
                //
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in trafficMetricsKeys)
                    {
                        e.Bind(trafficExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<TrafficAnalyticsConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // =====================================================
                // Finalize Configuration
                // =====================================================
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

/*

{
  "RabbitMQ": {

    "Host": "rabbitmq",
    "VirtualHost": "/",
    "Username": "stls_user",
    "Password": "stls_pass",

    "Exchanges": {
      "User": "user.exchange",
      "Traffic": "traffic.exchange",
      "Log": "log.exchange"
    },

    "Queues": {
      "User": {
        "NotificationRequest": "user-notification-request-queue"
      },
      "Traffic": {
        "Analytics": {
          "Notification": "traffic-analytics-notification-queue"
        }
      }
    },

    "RoutingKeys": {

      "UserRequests": [
        "user.notification.request.*"
      ],

      "TrafficAnalytics": [
        "traffic.analytics.*.congestion",
        "traffic.analytics.*.summary",
        "traffic.analytics.*.incident"
      ]
    }
  }
}

*/