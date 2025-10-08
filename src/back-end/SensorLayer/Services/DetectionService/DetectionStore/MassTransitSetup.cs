using MassTransit;
using RabbitMQ.Client;

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

                // =====================================
                // Connection (from configuration)
                // =====================================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // =====================================
                // Exchanges
                // =====================================
                var sensorExchange = rabbit["Exchanges:Sensor"]; // e.g. "sensor.exchange"
                var logExchange    = rabbit["Exchanges:Log"];    // e.g. "log.exchange"

                // =====================================
                // SENSOR DETECTION (Publish)
                // =====================================
                //
                // Topic pattern: sensor.detection.{intersection}.{event}
                // Example key:   sensor.detection.agiospyridonos.vehicle
                //
                cfg.Message<DetectionEventMessage>(m =>
                {
                    m.SetEntityName(sensorExchange);
                });

                cfg.Publish<DetectionEventMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================
                // LOGS (Publish)
                // =====================================
                cfg.Message<LogMessage>(m =>
                {
                    m.SetEntityName(logExchange);
                });

                cfg.Publish<LogMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
