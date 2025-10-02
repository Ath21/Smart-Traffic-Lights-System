using MassTransit;
using RabbitMQ.Client;
using SensorMessages;
using LogMessages;
using SensorMessages.SensorCount;

namespace SensorStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddSensorServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Sensor service only publishes, no consumers for now
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

                // =========================
                // SENSOR COUNTS (Publish)
                // =========================
                cfg.Message<CyclistCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<CyclistCountMessage>(e => e.ExchangeType = ExchangeType.Topic);
                cfg.Message<PedestrianCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<PedestrianCountMessage>(e => e.ExchangeType = ExchangeType.Topic);
                cfg.Message<VehicleCountMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<VehicleCountMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // Since this service does not consume, we don’t need to bind queues.
                // But leaving ConfigureEndpoints makes it future-proof if consumers are added later.
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
