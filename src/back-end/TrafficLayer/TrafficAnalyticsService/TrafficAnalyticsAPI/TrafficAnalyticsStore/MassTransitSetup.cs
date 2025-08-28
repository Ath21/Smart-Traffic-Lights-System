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
                var logsExchange     = rabbit["Exchanges:Logs"];
                var sensorExchange   = rabbit["Exchanges:Sensor"];
                var trafficExchange  = rabbit["Exchanges:Traffic"];

                // Queues
                var sensorQueue = rabbit["Queues:Sensor"];

                var vehicleCountKey         = rabbit["RoutingKeys:VehicleCount"] ?? "sensor.vehicle.count.{intersection_id}";
                var emergencyVehicleKey     = rabbit["RoutingKeys:VehicleEmergency"] ?? "sensor.vehicle.emergency.{intersection_id}";
                var publicTransportKey      = rabbit["RoutingKeys:PublicTransportRequest"] ?? "sensor.public_transport.request.{intersection_id}";
                var cyclistKey              = rabbit["RoutingKeys:CyclistRequest"] ?? "sensor.cyclist.request.{intersection_id}";
                var incidentDetectedKey     = rabbit["RoutingKeys:IncidentDetected"] ?? "sensor.incident.detected.{intersection_id}";
                var pedestrianKey           = rabbit["RoutingKeys:PedestrianRequest"] ?? "sensor.pedestrian.request.{intersection_id}";

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Direct);

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
                        s.RoutingKey = incidentDetectedKey.Replace("{intersection_id}", "*");
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
                // TRAFFIC EVENTS 
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
