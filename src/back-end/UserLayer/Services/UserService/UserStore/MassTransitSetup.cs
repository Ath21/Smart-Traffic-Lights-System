using MassTransit;
using RabbitMQ.Client;
using Messages.Log;
using Messages.User;
using Messages.Traffic;

namespace UserStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddUserServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
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
                var logExchange     = rabbit["Exchanges:Log"];
                var userExchange    = rabbit["Exchanges:User"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var userQueue    = rabbit["Queues:User:Notifications"];
                var trafficQueue = rabbit["Queues:Traffic:Analytics:User"];

                // Routing Keys
                var userNotifKeys     = rabbit.GetSection("RoutingKeys:UserNotifications").Get<string[]>() ?? Array.Empty<string>();
                var trafficAnalyticsKeys = rabbit.GetSection("RoutingKeys:TrafficAnalytics").Get<string[]>() ?? Array.Empty<string>();

                // =====================================================
                // [PUBLISH] LOGS
                // =====================================================
                //
                // Topic pattern : log.user.user.{type}
                // Example key   : log.user.user.audit
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
                // [PUBLISH] USER NOTIFICATION REQUESTS
                // =====================================================
                //
                // Topic pattern : user.notification.{type}
                // Example key   : user.notification.request
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
                // [PUBLISH] TRAFFIC LIGHT CONTROL COMMANDS
                // =====================================================
                //
                // Topic pattern : traffic.light.control.{intersection}.{light}
                // Example key   : traffic.light.control.agiou-spyridonos.101
                //
                cfg.Message<TrafficLightControlMessage>(m =>
                {
                    m.SetEntityName(trafficExchange);
                });
                cfg.Publish<TrafficLightControlMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================================
                // [CONSUME] USER NOTIFICATIONS
                // =====================================================
                //
                // Topic patterns:
                //   user.notification.{type}
                //   user.notification.public
                //
                cfg.ReceiveEndpoint(userQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in userNotifKeys)
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
                // Example key   : traffic.analytics.kentriki-pyli.summary
                //
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in trafficAnalyticsKeys)
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
        "Notifications": "user-notifications-queue"
      },
      "Traffic": {
        "Analytics": {
          "User": "traffic-analytics-user-queue"
        }
      }
    },

    "RoutingKeys": {

      "UserNotifications": [
        "user.notification.alert.*",
        "user.notification.public.broadcast"
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