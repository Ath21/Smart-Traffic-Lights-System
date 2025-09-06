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
            // Consumers
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
                var logExchange     = rabbit["Exchanges:Log"];
                var sensorExchange  = rabbit["Exchanges:Sensor"];
                var trafficExchange = rabbit["Exchanges:Traffic"];

                // Queues
                var sensorQueue = rabbit["Queues:Sensor:Analytics"];

                // Routing Keys (from SENSOR.EXCHANGE)
                var vehicleCountKey     = rabbit["RoutingKeys:Sensor:VehicleCount"];
                var emergencyKey        = rabbit["RoutingKeys:Sensor:EmergencyVehicle"];
                var publicTransportKey  = rabbit["RoutingKeys:Sensor:PublicTransport"];
                var pedestrianKey       = rabbit["RoutingKeys:Sensor:PedestrianCount"];
                var cyclistKey          = rabbit["RoutingKeys:Sensor:CyclistCount"];
                var incidentDetectedKey = rabbit["RoutingKeys:Sensor:IncidentDetected"];

                // Routing Keys (to TRAFFIC.EXCHANGE)
                var trafficCongKey   = rabbit["RoutingKeys:Traffic:Congestion"];
                var trafficSummaryKey= rabbit["RoutingKeys:Traffic:Summary"];
                var trafficIncKey    = rabbit["RoutingKeys:Traffic:Incident"];

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

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
                // TRAFFIC EVENTS (Publish)
                // =========================
                cfg.Message<TrafficMessages.TrafficCongestionMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficCongestionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficSummaryMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficSummaryMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<TrafficMessages.TrafficIncidentMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficMessages.TrafficIncidentMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
