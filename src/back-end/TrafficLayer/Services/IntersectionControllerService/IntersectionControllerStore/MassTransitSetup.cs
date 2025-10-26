using MassTransit;
using RabbitMQ.Client;
using IntersectionControllerStore.Consumers;
using IntersectionControllerStore.Consumers.Light;
using Messages.Traffic;
using Messages.Log;
using Messages.Traffic.Priority;
using Messages.Traffic.Light;

namespace IntersectionControllerStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddIntersectionControllerMassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // ===========================
            // Register Consumers
            // ===========================
            x.AddConsumer<LightScheduleConsumer>();
            x.AddConsumer<EmergencyVehicleDetectedConsumer>();
            x.AddConsumer<PublicTransportDetectedConsumer>();
            x.AddConsumer<IncidentDetectedConsumer>();
            x.AddConsumer<VehicleCountConsumer>();
            x.AddConsumer<PedestrianCountConsumer>();
            x.AddConsumer<CyclistCountConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // ===========================
                // Exchanges
                // ===========================
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var sensorExchange  = rabbit["Exchanges:Sensor"];
                var logExchange     = rabbit["Exchanges:Log"];

                // ===========================
                // Queues
                // ===========================
                var lightQueue     = rabbit["Queues:Traffic:LightSchedule"];
                var countQueue     = rabbit["Queues:Sensor:SensorCount"];
                var detectionQueue = rabbit["Queues:Sensor:DetectionEvents"];

                // ===========================
                // Routing Keys
                // ===========================
                var trafficLightKeys     = new[] { rabbit["RoutingKeys:Traffic:LightSchedule"] };
                var sensorCountKeys      = new[] { rabbit["RoutingKeys:Sensor:SensorCount"] };
                var sensorDetectionKeys  = new[] { rabbit["RoutingKeys:Sensor:DetectionEvents"] };

                // ===========================
                // PUBLISH CONFIGURATION
                // ===========================

                // Traffic Light Control
                cfg.Message<TrafficLightControlMessage>(m => m.SetEntityName(trafficExchange));
                cfg.Publish<TrafficLightControlMessage>(m => m.ExchangeType = ExchangeType.Topic);

                // Priority Event
                cfg.Message<PriorityEventMessage>(m => m.SetEntityName(trafficExchange));
                cfg.Publish<PriorityEventMessage>(m => m.ExchangeType = ExchangeType.Topic);

                // Priority Count
                cfg.Message<PriorityCountMessage>(m => m.SetEntityName(trafficExchange));
                cfg.Publish<PriorityCountMessage>(m => m.ExchangeType = ExchangeType.Topic);

                // Logs
                cfg.Message<LogMessage>(m => m.SetEntityName(logExchange));
                cfg.Publish<LogMessage>(m => m.ExchangeType = ExchangeType.Topic);

                // ===========================
                // CONSUMERS / ENDPOINTS
                // ===========================

                // Light Schedule (from Coordinator)
                cfg.ReceiveEndpoint(lightQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in trafficLightKeys)
                        e.Bind(trafficExchange, s => { s.ExchangeType = ExchangeType.Topic; s.RoutingKey = key; });

                    e.ConfigureConsumer<LightScheduleConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // Sensor Count (Vehicle, Pedestrian, Cyclist)
                cfg.ReceiveEndpoint(countQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in sensorCountKeys)
                        e.Bind(sensorExchange, s => { s.ExchangeType = ExchangeType.Topic; s.RoutingKey = key; });

                    e.ConfigureConsumer<VehicleCountConsumer>(context);
                    e.ConfigureConsumer<PedestrianCountConsumer>(context);
                    e.ConfigureConsumer<CyclistCountConsumer>(context);

                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                // Sensor Detection (Emergency Vehicle, Public Transport, Incident)
                cfg.ReceiveEndpoint(detectionQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    foreach (var key in sensorDetectionKeys)
                        e.Bind(sensorExchange, s => { s.ExchangeType = ExchangeType.Topic; s.RoutingKey = key; });

                    e.ConfigureConsumer<EmergencyVehicleDetectedConsumer>(context);
                    e.ConfigureConsumer<PublicTransportDetectedConsumer>(context);
                    e.ConfigureConsumer<IncidentDetectedConsumer>(context);

                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
