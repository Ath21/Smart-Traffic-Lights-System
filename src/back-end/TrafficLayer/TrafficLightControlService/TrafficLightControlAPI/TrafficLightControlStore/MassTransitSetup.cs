using System;
using LogMessages;
using MassTransit;
using RabbitMQ.Client;
using TrafficLightControlStore.Consumers;
using TrafficMessages;

namespace TrafficLightControlStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddTrafficLightControlMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Consumers
            x.AddConsumer<TrafficLightControlConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                // Exchanges
                var trafficExchange = rabbit["Exchanges:Traffic"];
                var logsExchange    = rabbit["Exchanges:Logs"];

                // Queues
                var controlQueue = rabbit["Queues:TrafficLightControl"];

                // Routing keys
                var controlKey = rabbit["RoutingKeys:TrafficLightControl"];

                // =========================
                // TRAFFIC LIGHT CONTROL (Consume)
                // =========================
                cfg.Message<TrafficLightControlMessage>(e => e.SetEntityName(trafficExchange));
                cfg.Publish<TrafficLightControlMessage>(e => e.ExchangeType = ExchangeType.Topic);

                cfg.ReceiveEndpoint(controlQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(trafficExchange, s =>
                    {
                        s.RoutingKey = controlKey.Replace("{intersection_id}", "*").Replace("{light_id}", "*");
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<TrafficLightControlConsumer>(context);
                });

                // =========================
                // LOGS (Publish)
                // =========================
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

