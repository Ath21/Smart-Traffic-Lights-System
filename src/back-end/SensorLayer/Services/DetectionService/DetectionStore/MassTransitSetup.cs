using MassTransit;
using RabbitMQ.Client;
using SensorMessages;

namespace DetectionStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddDetectionServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
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
                // SENSOR DETECTIONS (Publish)
                // =========================
                cfg.Message<SensorDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<SensorDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                // =========================
                // LOGS (Publish)
                // =========================
                cfg.Message<LogMessages.LogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<LogMessages.LogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
