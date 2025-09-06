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
            // Consumers
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
                var logExchange    = rabbit["Exchanges:Log"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var analyticsQueue = rabbit["Queues:Traffic:Analytics"];
                var priorityQueue  = rabbit["Queues:Priority:Coordinator"];

                // Routing Keys (Priority & Analytics)
                var emergencyKey  = rabbit["RoutingKeys:Priority:EmergencyVehicle"];
                var publicKey     = rabbit["RoutingKeys:Priority:PublicTransport"];
                var pedestrianKey = rabbit["RoutingKeys:Priority:Pedestrian"];
                var cyclistKey    = rabbit["RoutingKeys:Priority:Cyclist"];
                var incidentKey   = rabbit["RoutingKeys:Priority:Incident"];
                var congestionKey = rabbit["RoutingKeys:Traffic:Congestion"];

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC EVENTS (Consume - Analytics Queue)
                // =========================
                cfg.ReceiveEndpoint(analyticsQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = congestionKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficCongestionAlertConsumer>(context);
                });

                // =========================
                // TRAFFIC EVENTS (Consume - Priority Queue)
                // =========================
                cfg.ReceiveEndpoint(priorityQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

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

                    e.ConfigureConsumer<PriorityMessageConsumer>(context);
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
