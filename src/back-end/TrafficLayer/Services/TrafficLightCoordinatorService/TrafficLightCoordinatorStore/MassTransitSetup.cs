using MassTransit;
using RabbitMQ.Client;
using Messages.Log;
using Messages.Traffic;
using TrafficLightCoordinatorStore.Consumers;

namespace TrafficLightCoordinatorStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficCoordinatorMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // =====================================================
            // Register Consumers
            // =====================================================
            x.AddConsumer<PriorityEventConsumer>();
            x.AddConsumer<PriorityCountConsumer>();
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
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var logExchange     = rabbit["Exchanges:Log"];

                // Queues
                var priorityDetectionQueue = rabbit["Queues:Priority:Detection:Coordinator"];
                var priorityCountQueue     = rabbit["Queues:Priority:Count:Coordinator"];
                var analyticsQueue         = rabbit["Queues:Traffic:Analytics:Coordinator"];

                // Routing Keys
                var priorityDetectionKeys = rabbit.GetSection("RoutingKeys:PriorityDetection").Get<string[]>() ?? Array.Empty<string>();
                var priorityCountKeys     = rabbit.GetSection("RoutingKeys:PriorityCount").Get<string[]>() ?? Array.Empty<string>();
                var analyticsKeys         = rabbit.GetSection("RoutingKeys:TrafficAnalytics").Get<string[]>() ?? Array.Empty<string>();

                // =====================================================
                // [PUBLISH] TRAFFIC LIGHT UPDATE EVENTS
                // =====================================================
                //
                // Topic pattern : traffic.light.update.{intersection}.{light}
                // Example key   : traffic.light.update.agiou-spyridonos.agiou-spyridonos101
                //
                cfg.Message<TrafficLightScheduleMessage>(m =>
                {
                    m.SetEntityName(trafficExchange);
                });
                cfg.Publish<TrafficLightScheduleMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================================
                // [PUBLISH] LOG MESSAGES
                // =====================================================
                //
                // Topic pattern : log.traffic.coordinator.{type}
                // Example key   : log.traffic.coordinator.audit
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
                // [CONSUME] PRIORITY DETECTION EVENTS
                // =====================================================
                //
                // Topic pattern : priority.event.{intersection}.{type}
                // Example key   : priority.event.agiou-spyridonos.emergency-vehicle
                //
                cfg.ReceiveEndpoint(priorityDetectionQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in priorityDetectionKeys)
                    {
                        e.Bind(trafficExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<PriorityEventConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // =====================================================
                // [CONSUME] PRIORITY COUNT EVENTS
                // =====================================================
                //
                // Topic pattern : priority.count.{intersection}.{type}
                // Example key   : priority.count.kentriki-pyli.vehicle
                //
                cfg.ReceiveEndpoint(priorityCountQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in priorityCountKeys)
                    {
                        e.Bind(trafficExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<PriorityCountConsumer>(context);
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
                cfg.ReceiveEndpoint(analyticsQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in analyticsKeys)
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
      "Traffic": "traffic.exchange",
      "Log": "log.exchange"
    },

    "Queues": {
      "Priority": {
        "Detection": {
          "Coordinator": "traffic-priority-detection-coordinator-queue"
        },
        "Count": {
          "Coordinator": "traffic-priority-count-coordinator-queue"
        }
      },
      "Traffic": {
        "Analytics": {
          "Coordinator": "traffic-analytics-coordinator-queue"
        }
      }
    },

    "RoutingKeys": {

      "PriorityDetection": [
        "priority.event.*.emergency",
        "priority.event.*.public-transport",
        "priority.event.*.pedestrian",
        "priority.event.*.cyclist",
        "priority.event.*.incident"
      ],

      "PriorityCount": [
        "priority.count.*.vehicle",
        "priority.count.*.pedestrian",
        "priority.count.*.cyclist"
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