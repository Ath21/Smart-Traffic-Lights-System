using MassTransit;
using RabbitMQ.Client;

namespace SensorStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddSensorServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Sensor service publishes sensor counts & logs
            // No consumers (unless we later add commands for reset, calibration, etc.)

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
                var vehicleCountKey    = rabbit["RoutingKeys:Sensor:VehicleCount"];
                var pedestrianCountKey = rabbit["RoutingKeys:Sensor:PedestrianCount"];
                var cyclistCountKey    = rabbit["RoutingKeys:Sensor:CyclistCount"];

                // =========================
                // SENSOR COUNTS (Publish)
                // =========================
                cfg.Message<SensorMessages.VehicleCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.VehicleCountMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.PedestrianCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.PedestrianCountMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<SensorMessages.CyclistCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorMessages.CyclistCountMessage>(e => e.ExchangeType = ExchangeType.Topic);

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
