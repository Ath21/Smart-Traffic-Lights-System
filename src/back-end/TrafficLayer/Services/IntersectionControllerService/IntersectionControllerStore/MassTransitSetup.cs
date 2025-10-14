using MassTransit;
using RabbitMQ.Client;
using IntersectionControllerStore.Consumers;
using Messages.Traffic;
using Messages.Log;

namespace IntersectionControllerStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddIntersectionControllerMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // =====================================================
            // Register Consumers
            // =====================================================
            x.AddConsumer<LightScheduleConsumer>();
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

                // ===============================
                // Exchanges, Queues and Routing Keys
                // ===============================
                // Exchanges
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var sensorExchange  = rabbit["Exchanges:Sensor"];
                var logExchange     = rabbit["Exchanges:Log"];

                // Queues
                var trafficQueue = rabbit["Queues:Traffic:LightUpdate"];
                var sensorCountQueue = rabbit["Queues:Sensor:Count"];
                var detectionQueue   = rabbit["Queues:Sensor:Detection"];

                // Routing keys
                var trafficKeys   = rabbit.GetSection("RoutingKeys:Traffic").Get<string[]>() ?? Array.Empty<string>();
                var countKeys     = rabbit.GetSection("RoutingKeys:SensorCount").Get<string[]>() ?? Array.Empty<string>();
                var detectionKeys = rabbit.GetSection("RoutingKeys:DetectionEvent").Get<string[]>() ?? Array.Empty<string>();

                // ===============================
                // [PUBLISH] TRAFFIC LIGHT CONTROL (agiou-spyridonos.agiou-spyridonos101, agiou-spyridonos.dimitsanas102, ...)
                // ===============================
                // 
                // Topic pattern : traffic.light.control.{intersection}.{light}
                // Example key   : traffic.light.control.agiou-spyridonos.agiou-spyridonos101
                //
                cfg.Message<TrafficLightControlMessage>(m =>
                {
                    m.SetEntityName(trafficExchange);
                });
                cfg.Publish<TrafficLightControlMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // ===============================
                // [PUBLISH] PRIORITY EVENTS (Emergency Vehicle, Public Transport, Incident)
                // ===============================
                //
                // Topic pattern : priority.detection.{intersection}.{event}
                // Example key   : priority.detection.agiou-spyridonos.emergency-vehicle
                //
                cfg.Message<PriorityEventMessage>(m =>
                {
                    m.SetEntityName(trafficExchange);
                });
                cfg.Publish<PriorityEventMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // ===============================
                // [PUBLISH] PRIORITY COUNT (Vehicles, Pedestrians, Cyclists) 
                // ===============================
                //
                // Topic pattern : priority.count.{intersection}.{type}
                // Example key   : priority.count.kentriki-pyli.pedestrian
                //
                cfg.Message<PriorityCountMessage>(m =>
                {
                    m.SetEntityName(trafficExchange);
                });
                cfg.Publish<PriorityCountMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // ===============================
                // [PUBLISH] LOGS (Audit, Error, Failover)
                // ===============================
                //
                // Topic pattern : log.{layer}.{service}.{type}
                // Example key   : log.sensor.intersection-controller.audit
                //
                cfg.Message<LogMessage>(m =>
                {
                    m.SetEntityName(logExchange);
                });
                cfg.Publish<LogMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // ===============================
                // [CONSUME] TRAFFIC LIGHT UPDATE (agiou-spyridonos, kentriki-pyli, ...)
                // ===============================
                // 
                // Topic pattern : traffic.light.update.{intersection}
                // Example key   : traffic.light.update.agiou-spyridonos
                //
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in trafficKeys)
                    {
                        e.Bind(trafficExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    e.ConfigureConsumer<LightScheduleConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // ===============================
                // [CONSUME] SENSOR COUNT (Vehicles, Pedestrians, Cyclists)
                // ===============================
                //
                // Topic pattern : sensor.count.{intersection}.{type}
                // Example key   : sensor.count.kentriki-pyli.pedestrian
                //
                cfg.ReceiveEndpoint(sensorCountQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in countKeys)
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

                // ===============================
                // [CONSUME] SENSOR DETECTION (Emergency Vehicle, Public Transport, Incident)
                // ===============================
                //
                // Topic pattern : sensor.detection.{intersection}.{event}
                // Example key   : sensor.detection.agiou-spyridonos.emergency-vehicle
                //  
                cfg.ReceiveEndpoint(detectionQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in detectionKeys)
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
      "Traffic": "traffic.exchange",
      "Sensor": "sensor.exchange",
      "Log": "log.exchange"
    },

    "Queues": {
      "Traffic": {
        "LightUpdate": "intersection-traffic-update-queue"
      },
      "Sensor": {
        "Count": "intersection-sensor-count-queue",
        "Detection": "intersection-detection-queue"
      }
    },

    "RoutingKeys": {

      "Traffic": [
        "traffic.light.update.*"
      ],

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