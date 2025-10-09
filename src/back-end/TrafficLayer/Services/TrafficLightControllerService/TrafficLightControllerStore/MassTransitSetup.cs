using MassTransit;
using RabbitMQ.Client;
using Messages.Log;
using Messages.Traffic;
using TrafficLightControllerStore.Consumers;

namespace TrafficLightControllerStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficLightControllerMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // =====================================================
            // Register Consumers
            // =====================================================
            x.AddConsumer<TrafficLightControlConsumer>();

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
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var logExchange = rabbit["Exchanges:Log"];

                // Queues
                var controlQueue = rabbit["Queues:Traffic:LightControl"];

                // Routing Keys
                var controlKey = rabbit["RoutingKeys:Traffic:LightControl"];

                // =====================================================
                // [CONSUME] TRAFFIC LIGHT CONTROL COMMANDS
                // =====================================================
                //
                // Topic pattern : traffic.light.control.{intersection}.{light}
                // Example key   : traffic.light.control.kentriki-pyli.kentriki-pyli201
                //
                cfg.ReceiveEndpoint(controlQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = controlKey
                            .Replace("{intersection}", "*")
                            .Replace("{light}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficLightControlConsumer>(context);

                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // =====================================================
                // [PUBLISH] LOG MESSAGES
                // =====================================================
                //
                // Topic pattern : log.traffic.light-controller.{type}
                // Example key   : log.traffic.light-controller.audit
                //
                cfg.Message<LogMessage>(m =>
                {
                    m.SetEntityName(logExchange);
                });
                cfg.Publish<LogMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
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
      "Traffic": {
        "LightControl": "traffic-light-control-queue"
      }
    },

    "RoutingKeys": {
      "Traffic": {
        "LightControl": "traffic.light.control.{intersection}.{light}"
      }
    }
  }
}

*/