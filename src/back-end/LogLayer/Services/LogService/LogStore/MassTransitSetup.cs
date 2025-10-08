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
            // Single unified consumer
            x.AddConsumer<LogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                // ===============================
                // Connection (from configuration)
                // ===============================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchange name and queue name
                var logsExchange = rabbit["Exchanges:Log"];
                var logQueue = rabbit["Queues:UnifiedLogQueue"];

                // All routing keys (defined in appsettings or ConfigMap)
                var routingKeys = rabbit.GetSection("RoutingKeys:Log").Get<string[]>() ?? Array.Empty<string>();

                // ===============================
                // Unified Log Consumer Queue
                // ===============================
                cfg.ReceiveEndpoint(logQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    foreach (var key in routingKeys)
                    {
                        // Bind each routing key to same queue
                        e.Bind(logsExchange, s =>
                        {
                            s.RoutingKey = key;
                            s.ExchangeType = ExchangeType.Topic;
                        });
                    }

                    // Register single consumer
                    e.ConfigureConsumer<LogConsumer>(context);

                    // (Optional tuning)
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
      "UnifiedLogQueue": "log-service"
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