using MassTransit;
using RabbitMQ.Client;
using IntersectionControllerStore.Consumers;
using TrafficMessages;
using SensorMessages;

namespace IntersectionControllerStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddIntersectionControllerMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
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
                var logExchange     = rabbit["Exchanges:Log"];

                // Queues
                var trafficQueue = rabbit["Queues:Traffic:LightUpdate"];
                var sensorQueue  = rabbit["Queues:Sensor:Intersection"];

                // Routing Keys
                var lightUpdateKey   = rabbit["RoutingKeys:Traffic:LightUpdate"];
                var vehicleCountKey  = rabbit["RoutingKeys:Sensor:VehicleCount"];
                var emergencyKey     = rabbit["RoutingKeys:Sensor:EmergencyVehicle"];
                var publicTransportKey = rabbit["RoutingKeys:Sensor:PublicTransport"];
                var pedestrianKey    = rabbit["RoutingKeys:Sensor:PedestrianCount"];
                var cyclistKey       = rabbit["RoutingKeys:Sensor:CyclistCount"];
                var incidentKey      = rabbit["RoutingKeys:Sensor:IncidentDetected"];

                // =========================
                // TRAFFIC LIGHT CONTROL → publish
                // =========================
                cfg.Message<TrafficLightControlMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficLightControlMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // PRIORITY EVENTS → publish
                // =========================
                cfg.Message<PriorityMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<PriorityMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // LOGS → publish
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // TRAFFIC LIGHT UPDATES (Consume)
                // =========================
                cfg.ReceiveEndpoint(trafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = lightUpdateKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficLightUpdateConsumer>(context);
                });

                // =========================
                // SENSOR EVENTS (Consume)
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
                        s.RoutingKey = emergencyKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = publicTransportKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = pedestrianKey.Replace("{intersection_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = cyclistKey.Replace("{intersection_id}", "*");
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
