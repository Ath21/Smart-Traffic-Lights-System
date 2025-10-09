using MassTransit;
using Messages.Log;
using Messages.Sensor;
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
                // Connection with RabbitMQ
                // =====================================
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // =====================================
                // Exchanges
                // =====================================
                var sensorExchange = rabbit["Exchanges:Sensor"]; 
                var logExchange    = rabbit["Exchanges:Log"];    

                // =====================================
                // [PUBLISH] SENSOR DETECTION (Emergency Vehicle, Public Transport, Incident)
                // =====================================
                //
                // Topic pattern : sensor.detection.{intersection}.{event}
                // Example key   : sensor.detection.agiou-spyridonos.emergency-vehicle
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
                // [PUBLISH] LOGS (Audit, Error, Failover)
                // =====================================
                //
                // Topic pattern : log.{layer}.{service}.{type}
                // Example key   : log.sensor.detection.audit
                //
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

/*

{
  "RabbitMQ": {
    "Host": "rabbitmq",
    "VirtualHost": "/",
    "Username": "stls_user",
    "Password": "stls_pass",
    "Exchanges": {
      "Sensor": "sensor.exchange",
      "Log": "log.exchange"
    }
  }
}

*/