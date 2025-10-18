using MassTransit;
using RabbitMQ.Client;
using LogStore.Consumers;
using Messages;
using Messages.Log;

namespace LogStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddLogServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<LogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");
                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h => { h.Username(rabbit["Username"]); h.Password(rabbit["Password"]); });

                var logExchange     = rabbit["Exchanges:Log"];
                var logUserQueue    = rabbit["Queues:Log:User"];
                var logTrafficQueue = rabbit["Queues:Log:Traffic"];
                var logSensorQueue  = rabbit["Queues:Log:Sensor"];
                var rkUser          = rabbit["RoutingKeys:Log:User"];
                var rkTraffic       = rabbit["RoutingKeys:Log:Traffic"];
                var rkSensor        = rabbit["RoutingKeys:Log:Sensor"];

                cfg.Publish<BaseMessage>(m => m.Exclude = true);
                cfg.Message<LogMessage>(m => m.SetEntityName(logExchange));
                cfg.Publish<LogMessage>(m => m.ExchangeType = ExchangeType.Topic);

                cfg.ReceiveEndpoint(logUserQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind(logExchange, s =>
                    {
                        s.RoutingKey = rkUser;
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.ConfigureConsumer<LogConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });
                cfg.ReceiveEndpoint(logTrafficQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind(logExchange, s =>
                    {
                        s.RoutingKey = rkTraffic;
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.ConfigureConsumer<LogConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });
                cfg.ReceiveEndpoint(logSensorQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind(logExchange, s =>
                    {
                        s.RoutingKey = rkSensor;
                        s.ExchangeType = ExchangeType.Topic;
                    });
                    e.ConfigureConsumer<LogConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });
            });
        });
        return services;
    }
}
