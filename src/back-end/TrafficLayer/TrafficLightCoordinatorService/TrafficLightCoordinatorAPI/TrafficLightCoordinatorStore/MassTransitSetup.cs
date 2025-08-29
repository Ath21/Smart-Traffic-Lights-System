using MassTransit;
using RabbitMQ.Client;
using TrafficLightCoordinatorStore.Consumers;

namespace TrafficLightCoordinatorStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficCoordinatorMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register Consumers
            x.AddConsumer<PriorityMessageConsumer>();
            x.AddConsumer<TrafficCongestionAlertConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var logsExchange    = rabbit["Exchanges:Logs"] ?? "LOG.EXCHANGE";
                var trafficExchange = rabbit["Exchanges:Traffic"] ?? "TRAFFIC.EXCHANGE";

                // Queue
                var trafficQueue = rabbit["Queues:Traffic"] ?? "traffic.coordinator_service.queue";

                // Routing keys
                var emergencyKey   = rabbit["RoutingKeys:Emergency"]       ?? "priority.emergency.{intersection_id}";
                var publicKey      = rabbit["RoutingKeys:PublicTransport"] ?? "priority.public_transport.{intersection_id}";
                var pedestrianKey  = rabbit["RoutingKeys:Pedestrian"]      ?? "priority.pedestrian.{intersection_id}";
                var cyclistKey     = rabbit["RoutingKeys:Cyclist"]         ?? "priority.cyclist.{intersection_id}";
                var incidentKey    = rabbit["RoutingKeys:Incident"]        ?? "priority.incident.{intersection_id}";
                var congestionKey  = rabbit["RoutingKeys:Congestion"]      ?? "traffic.analytics.congestion.{intersection_id}";

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // TRAFFIC EVENTS (Consume)
                // =========================
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    // Bind each priority/congestion routing key
                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = emergencyKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = publicKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = pedestrianKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = cyclistKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = incidentKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = congestionKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Consumers
                    e.ConfigureConsumer<PriorityMessageConsumer>(context);
                    e.ConfigureConsumer<TrafficCongestionAlertConsumer>(context);
                });

                // =========================
                // TRAFFIC EVENTS (Publish)
                // =========================
                cfg.Message<TrafficMessages.TrafficLightUpdateMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightUpdateMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
