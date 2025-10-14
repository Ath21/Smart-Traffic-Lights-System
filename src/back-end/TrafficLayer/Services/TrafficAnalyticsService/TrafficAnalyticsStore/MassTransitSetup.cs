using MassTransit;
using RabbitMQ.Client;
using Messages.Log;
using Messages.Traffic;
using TrafficAnalyticsStore.Consumers;

namespace TrafficAnalyticsStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficAnalyticsMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // =====================================================
            // Register Consumers
            // =====================================================
            x.AddConsumer<SensorCountConsumer>();
            x.AddConsumer<DetectionEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                // ===============================
                // Connection with RabbitMQ
                // ===============================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // =====================================================
                // Exchanges, Queues, and Routing Keys
                // =====================================================
                // Exchanges
                var sensorExchange  = rabbit["Exchanges:Sensor"];
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var logExchange     = rabbit["Exchanges:Log"];

                // Queues 
                var sensorCountQueue = rabbit["Queues:Sensor:Count"];
                var detectionQueue   = rabbit["Queues:Detection:Event"];

                // Routing Keys
                var sensorCountKeys     = rabbit.GetSection("RoutingKeys:Sensor:Count").Get<string[]>() ?? Array.Empty<string>();
                var sensorDetectionKeys = rabbit.GetSection("RoutingKeys:Detection:Event").Get<string[]>() ?? Array.Empty<string>();

                // =====================================================
                // [PUBLISH] TRAFFIC ANALYTICS METRICS (Congestion, Summary, Incident)
                // =====================================================
                //
                // Topic pattern : traffic.analytics.{intersection}.{metric}
                // Example key   : traffic.analytics.agiou-spyridonos.congestion
                //
                cfg.Message<TrafficAnalyticsMessage>(m =>
                {
                    m.SetEntityName(trafficExchange);
                });
                cfg.Publish<TrafficAnalyticsMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================================
                // [PUBLISH] LOGS (Audit, Error, Failover)
                // =====================================================
                //
                // Topic pattern : log.traffic.analytics.{type}
                // Example key   : log.traffic.analytics.error
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
                // [CONSUME] SENSOR COUNT (Vehicles, Pedestrians, Cyclists)
                // =====================================================
                //
                // Topic pattern : sensor.count.{intersection}.{type}
                // Example key   : sensor.count.kentriki-pyli.vehicle
                //
                cfg.ReceiveEndpoint(sensorCountQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in sensorCountKeys)
                    {
                        e.Bind(sensorExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }
                    e.ConfigureConsumer<SensorCountConsumer>(context);

                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // =====================================================
                // [CONSUME] DETECTION EVENTS (Emergency Vehicle, Public Transport, Incident)
                // =====================================================
                //
                // Topic pattern : sensor.detection.{intersection}.{event}
                // Example key   : sensor.detection.agiou-pyridonos.emergency-vehicle
                //
                cfg.ReceiveEndpoint(detectionQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in sensorDetectionKeys)
                    {
                        e.Bind(sensorExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }
                    e.ConfigureConsumer<DetectionEventConsumer>(context);

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
      "Sensor": "sensor.exchange",
      "Traffic": "traffic.exchange",
      "Log": "log.exchange"
    },

    "Queues": {
      "Sensor": {
        "Count": {
          "TrafficAnalytics": "traffic-analytics-sensor-count-queue"
        },
        "Detection": {
          "TrafficAnalytics": "traffic-analytics-detection-queue"
        }
      }
    },

    "RoutingKeys": {

      "SensorCount": [
        "sensor.count.*.vehicle",
        "sensor.count.*.pedestrian",
        "sensor.count.*.cyclist"
      ],

      "SensorDetection": [
        "sensor.detection.*.emergency-vehicle",
        "sensor.detection.*.public-transport",
        "sensor.detection.*.incident"
      ]
    }
  }
}


*/