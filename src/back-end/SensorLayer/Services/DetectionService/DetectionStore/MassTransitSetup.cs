using MassTransit;
using RabbitMQ.Client;

namespace DetectionStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddDetectionServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Detection doesn’t consume events — it only publishes sensor events and logs.
            // If later you add consumers (like commands to reset counts), register them here.

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var sensorExchange = rabbit["Exchanges:Sensor"];
                var logExchange    = rabbit["Exchanges:Log"];

                // Routing keys (publish)
                var emergencyKey   = rabbit["RoutingKeys:Sensor:EmergencyVehicle"];
                var publicKey      = rabbit["RoutingKeys:Sensor:PublicTransport"];
                var incidentKey    = rabbit["RoutingKeys:Sensor:IncidentDetected"];

                // =========================
                // SENSOR EVENTS (Publish)
                // =========================
                cfg.Message<SensorMessages.EmergencyVehicleMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.EmergencyVehicleMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.PublicTransportDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.PublicTransportDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.IncidentDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.IncidentDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessages.AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<LogMessages.ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
