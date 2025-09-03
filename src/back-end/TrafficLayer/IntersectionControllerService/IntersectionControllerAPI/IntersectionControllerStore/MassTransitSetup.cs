using MassTransit;
using RabbitMQ.Client;
using IntersectionControllerStore.Consumers;
using TrafficMessages;
using SensorMessages; // namespace where TrafficLightUpdateMessage and sensor messages live

namespace IntersectionControllerStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddIntersectionControllerMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Consumers
            x.AddConsumer<TrafficLightUpdateConsumer>();
            x.AddConsumer<SensorDataConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var sensorExchange  = rabbit["Exchanges:Sensor"];
                var logsExchange     = rabbit["Exchanges:Logs"];

                // Queues
                var trafficQueue = rabbit["Queues:TrafficIntersection"];
                var sensorQueue  = rabbit["Queues:SensorIntersection"];

                // Routing keys
                var trafficLightUpdateKey = rabbit["RoutingKeys:TrafficLightUpdate"];

                var vehicleCountKey     = rabbit["RoutingKeys:SensorVehicleCount"];
                var emergencyVehicleKey = rabbit["RoutingKeys:SensorEmergencyVehicle"];
                var publicTransportKey  = rabbit["RoutingKeys:SensorPublicTransport"];
                var cyclistKey          = rabbit["RoutingKeys:SensorCyclist"];
                var pedestrianKey       = rabbit["RoutingKeys:SensorPedestrian"];
                var incidentKey         = rabbit["RoutingKeys:SensorIncident"];

                // =========================
                // TRAFFIC COMMANDS -> publish to TRAFFIC.EXCHANGE
                // =========================
                cfg.Message<TrafficMessages.TrafficLightControlMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightControlMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // ==========================
                // PRIORITY MESSAGES -> publish to TRAFFIC.EXCHANGE
                // ==========================
                cfg.Message<TrafficMessages.PriorityMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.PriorityMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // ==========================
                // LOGS -> publish to LOG.EXCHANGE
                // ==========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // ==========================
                // TRAFFIC LIGHT PATTERNS
                // ==========================
                cfg.Message<TrafficMessages.TrafficLightUpdateMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficLightUpdateMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // ==========================
                // SENSOR DATA
                // ==========================
                cfg.Message<SensorMessages.VehicleCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.VehicleCountMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.EmergencyVehicleMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.EmergencyVehicleMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.PublicTransportMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.PublicTransportMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.CyclistDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.CyclistDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.PedestrianDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.PedestrianDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.IncidentDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.IncidentDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC LIGHT UPDATES
                // =========================
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = trafficLightUpdateKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficLightUpdateConsumer>(context);
                });

                // =========================
                // SENSOR EVENTS
                // =========================
                cfg.ReceiveEndpoint(sensorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = vehicleCountKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = emergencyVehicleKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = publicTransportKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = cyclistKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = pedestrianKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = incidentKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<SensorDataConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
