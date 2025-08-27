using MassTransit;
using RabbitMQ.Client;
using TrafficAnalyticsStore.Consumers;

namespace TrafficAnalyticsStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficAnalyticsMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register Consumers
            x.AddConsumer<VehicleCountConsumer>();
            x.AddConsumer<EmergencyVehicleConsumer>();
            x.AddConsumer<PublicTransportConsumer>();
            x.AddConsumer<PedestrianDetectionConsumer>();
            x.AddConsumer<CyclistDetectionConsumer>();
            x.AddConsumer<IncidentDetectionConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var logsExchange = rabbit["Exchanges:Logs"];
                var sensorExchange = rabbit["Exchanges:Sensor"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var analyticsQueue = rabbit["Queues:Analytics"];

                // Routing keys
                var vehicleCountKey = rabbit["RoutingKeys:VehicleCount"];
                var emergencyVehicleKey = rabbit["RoutingKeys:EmergencyVehicle"];
                var publicTransportKey = rabbit["RoutingKeys:PublicTransport"];
                var pedestrianKey = rabbit["RoutingKeys:Pedestrian"];
                var cyclistKey = rabbit["RoutingKeys:Cyclist"];
                var incidentDetectedKey = rabbit["RoutingKeys:IncidentDetected"];

                var congestionPrefix = rabbit["RoutingKeys:TrafficCongestionPrefix"];
                var incidentPrefix = rabbit["RoutingKeys:TrafficIncidentPrefix"];
                var summaryPrefix = rabbit["RoutingKeys:TrafficSummaryPrefix"];

                // =========================
                // LOGS (Publish only)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                // =========================
                // SENSOR EVENTS (Consume)
                // =========================
                cfg.ReceiveEndpoint(analyticsQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    // Bind vehicle counts
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = vehicleCountKey ?? "sensor.vehicle.count.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Bind emergency vehicle
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = emergencyVehicleKey ?? "sensor.vehicle.emergency.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Bind public transport
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = publicTransportKey ?? "sensor.public_transport.request.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Bind pedestrian
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = pedestrianKey ?? "sensor.pedestrian.request.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Bind cyclist
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = cyclistKey ?? "sensor.cyclist.request.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Bind incidents
                    e.Bind(sensorExchange, s =>
                    {
                        s.RoutingKey = incidentDetectedKey ?? "sensor.incident.detected.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    // Consumers
                    e.ConfigureConsumer<VehicleCountConsumer>(context);
                    e.ConfigureConsumer<EmergencyVehicleConsumer>(context);
                    e.ConfigureConsumer<PublicTransportConsumer>(context);
                    e.ConfigureConsumer<PedestrianDetectionConsumer>(context);
                    e.ConfigureConsumer<CyclistDetectionConsumer>(context);
                    e.ConfigureConsumer<IncidentDetectionConsumer>(context);
                });

                // =========================
                // TRAFFIC EVENTS (Publish)
                // =========================
                cfg.Message<TrafficMessages.TrafficSummaryMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficSummaryMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficCongestionMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficCongestionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficIncidentMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficIncidentMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
