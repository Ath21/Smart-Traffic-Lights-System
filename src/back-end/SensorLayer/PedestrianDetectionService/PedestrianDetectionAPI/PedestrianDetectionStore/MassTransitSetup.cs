using MassTransit;
using RabbitMQ.Client;
using SensorMessages;
using LogMessages;

namespace PedestrianDetectionStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddPedestrianDetectionMassTransit(this IServiceCollection services, IConfiguration configuration)
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

                var sensorExchange = rabbit["Exchanges:Sensor"];
                var logsExchange   = rabbit["Exchanges:Logs"];

                cfg.Message<PedestrianDetectionMessage>(e => e.SetEntityName(sensorExchange));
                cfg.Publish<PedestrianDetectionMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<AuditLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<ErrorLogMessage>(e => e.SetEntityName(logsExchange));
                cfg.Publish<ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
