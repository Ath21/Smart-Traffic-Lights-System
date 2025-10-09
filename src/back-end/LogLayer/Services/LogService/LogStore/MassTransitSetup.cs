using MassTransit;
using RabbitMQ.Client;
using LogStore.Consumers;

namespace LogStore;

public static class MassTransitSetup
{
  public static IServiceCollection AddLogServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddMassTransit(x =>
    {
      // =====================================================
      // Register Consumers
      // =====================================================
      x.AddConsumer<LogConsumer>();

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
        var logExchange = rabbit["Exchanges:Log"];

        // Queues
        var logQueue = rabbit["Queues:Log"];

        // Routing keys 
        var routingKeys = rabbit.GetSection("RoutingKeys:Log").Get<string[]>() ?? Array.Empty<string>();

        // ===============================
        // [CONSUME] LOGS (Audit, Error, Failover)
        // ===============================
        //  
        // Topic pattern : log.{layer}.{service}.{type}
        // Example key   : log.sensor.intersection-controller.failover
        //
        cfg.ReceiveEndpoint(logQueue, e =>
        {
          e.ConfigureConsumeTopology = false;

          foreach (var key in routingKeys)
          {
            e.Bind(logExchange, s =>
            {
              s.RoutingKey = key;
              s.ExchangeType = ExchangeType.Topic;
            });
          }
          e.ConfigureConsumer<LogConsumer>(context);
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
      "Log": "log.exchange"
    },

    "Queues": {
      "Log": "log-service"
    },

    "RoutingKeys": {
      "Log": [
        "log.sensor.*.*",
        "log.traffic.*.*",
        "log.user.*.*"
      ]
    }
  }
}

*/