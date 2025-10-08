using MassTransit;
using RabbitMQ.Client;

namespace SensorStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddSensorServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Sensor service only publishes messages (no consumers yet)
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                // ===============================
                // Connection (injected from config)
                // ===============================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // ===============================
                // Exchanges
                // ===============================
                var sensorExchange = rabbit["Exchanges:Sensor"]; // e.g. "sensor.exchange"
                var logExchange    = rabbit["Exchanges:Log"];    // e.g. "log.exchange"

                // ===============================
                // SENSOR COUNTS (Publish)
                // ===============================
                //
                // Topic pattern: sensor.count.{intersection}.{type}
                // Example keys:
                //   sensor.count.agiospyridonos.vehicle
                //   sensor.count.kentrikipyli.pedestrian
                //
                cfg.Message<SensorCountMessage>(m => m.SetEntityName(sensorExchange));
                cfg.Publish<SensorCountMessage>(m => m.ExchangeType = ExchangeType.Topic);

                // ===============================
                // LOGS (Publish)
                // ===============================
                cfg.Message<LogMessage>(m => m.SetEntityName(logExchange));
                cfg.Publish<LogMessage>(m => m.ExchangeType = ExchangeType.Topic);

                // Since this service is publish-only, no queues are declared.
                // Keeping ConfigureEndpoints allows easy consumer extension later.
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
