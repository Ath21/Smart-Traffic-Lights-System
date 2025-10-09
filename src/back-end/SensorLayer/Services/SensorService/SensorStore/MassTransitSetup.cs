using MassTransit;
using Messages.Log;
using Messages.Sensor;
using RabbitMQ.Client;

namespace SensorStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddSensorServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
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
                // [PUBLISH] SENSOR COUNT (Vehicles, Pedestrians, Cyclists)
                // =====================================
                //
                // Topic pattern : sensor.count.{intersection}.{type}
                // Example key   : sensor.count.kentriki-pyli.pedestrian
                //
                cfg.Message<SensorCountMessage>(m =>
                {
                    m.SetEntityName(sensorExchange);
                });

                cfg.Publish<SensorCountMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                // =====================================
                // [PUBLISH] LOGS (Audit, Error, Failover)
                // =====================================
                //
                // Topic pattern : log.{layer}.{service}.{type}
                // Example key   : log.sensor.sensor.error
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
