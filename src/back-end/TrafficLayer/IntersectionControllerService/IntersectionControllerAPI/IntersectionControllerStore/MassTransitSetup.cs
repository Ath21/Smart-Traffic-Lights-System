using MassTransit;
using RabbitMQ.Client;
using IntersectionControlStore.Consumers;
using IntersectionControllerStore.Consumers;

namespace IntersectionControlStore;

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

                // Queues
                var trafficQueue = rabbit["Queues:TrafficIntersection"];
                var sensorQueue  = rabbit["Queues:SensorIntersection"];

                // Routing keys
                var trafficLightUpdateKey   = rabbit["RoutingKeys:TrafficLightUpdate"];
                var vehicleCountKey         = rabbit["RoutingKeys:SensorVehicleCount"];
                var emergencyVehicleKey     = rabbit["RoutingKeys:SensorEmergencyVehicle"];
                var publicTransportKey      = rabbit["RoutingKeys:SensorPublicTransport"];
                var cyclistKey              = rabbit["RoutingKeys:SensorCyclist"];
                // if later needed, add pedestrian + incident

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

                    // TODO: extend with pedestrian + incident if you enable them

                    e.ConfigureConsumer<SensorDataConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
