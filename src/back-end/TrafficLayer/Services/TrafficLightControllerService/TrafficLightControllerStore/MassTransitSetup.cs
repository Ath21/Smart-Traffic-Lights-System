using LogMessages;
using MassTransit;
using RabbitMQ.Client;
using TrafficLightControllerStore.Consumers;
using TrafficMessages;

namespace TrafficLightControllerStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficLightControlMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TrafficLightControlConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                var trafficExchange = rabbit["Exchanges:Traffic"];
                var logExchange     = rabbit["Exchanges:Log"];
                var controlQueue    = rabbit["Queues:Traffic:LightControl"];
                var controlKey      = rabbit["RoutingKeys:Traffic:LightControl"];

                // =========================
                // TRAFFIC LIGHT CONTROL
                // =========================
                cfg.Message<TrafficLightControlMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficLightControlMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ReceiveEndpoint(controlQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = controlKey.Replace("{intersection}", "*").Replace("{light}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.ConfigureConsumer<TrafficLightControlConsumer>(context);
                });

                // =========================
                // LOGGING (Audit, Error, Failover)
                // =========================
                cfg.Message<AuditLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<AuditLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<ErrorLogMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<ErrorLogMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.Message<FailoverMessage>(e => e.SetEntityName(logExchange));
                cfg.Publish<FailoverMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
