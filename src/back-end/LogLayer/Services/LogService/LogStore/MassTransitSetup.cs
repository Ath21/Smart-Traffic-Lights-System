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
                // RabbitMQ Connection
                // ===============================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // ===============================
                // Exchanges, Queues, Routing Keys
                // ===============================
                var logExchange = rabbit["Exchanges:Log"];
                var logQueue = rabbit["Queues:Log"];
                var routingKeys = rabbit.GetSection("RoutingKeys:Log").Get<string[]>() ?? Array.Empty<string>();

                // ===============================
                // Unified Consumer: Log Service
                // ===============================
                // Topic pattern: log.{layer}.{service}.{type}
                // Example keys: log.sensor.detection-service.audit
                //               log.traffic.intersection-controller.error
                //               log.user.notification-service.failover
                // ===============================
                cfg.ReceiveEndpoint(logQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    // Bind all unified routing keys to one queue
                    foreach (var key in routingKeys)
                    {
                        e.Bind(logExchange, s =>
                        {
                            s.ExchangeType = ExchangeType.Topic;
                            s.RoutingKey = key;
                        });
                    }

                    // Configure consumer and concurrency
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
