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

                var intersection = configuration["Intersection:Name"]?.ToLower().Replace(" ", "-") ?? "default";
                var light = configuration["TrafficLight:Id"] ?? "0"; // optional, if you have per-light ID

                // ===============================
                // Connection with RabbitMQ
                // ===============================
                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // ===============================
                // Exchanges, Queues, Routing Keys
                // ===============================
                var trafficExchange = rabbit["Exchanges:Traffic"]; // TRAFFIC.EXCHANGE
                var logExchange = rabbit["Exchanges:Log"];         // LOG.EXCHANGE

                // Replace placeholders in queue name
                var controlQueue = rabbit["Queues:Traffic:LightControl"]?
                    .Replace("{intersection}", intersection)
                    .Replace("{light}", light);

                var controlKey = rabbit["RoutingKeys:Traffic:LightControl"]?
                    .Replace("{intersection}", intersection)
                    .Replace("{light}", light);

                var logKey = rabbit["RoutingKeys:Log:LightController"]; // log.traffic.light-controller.{type}

                // =====================================================
                // [CONSUME] TRAFFIC LIGHT CONTROL COMMANDS
                // =====================================================
                cfg.ReceiveEndpoint(controlQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = controlKey;
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficLightControlConsumer>(context);

                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // =====================================================
                // [PUBLISH] LOG MESSAGES
                // =====================================================
                cfg.Message<LogMessage>(m => m.SetEntityName(logExchange));
                cfg.Publish<LogMessage>(m => m.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
