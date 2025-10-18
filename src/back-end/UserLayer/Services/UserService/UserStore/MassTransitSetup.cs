using MassTransit;
using RabbitMQ.Client;
using Messages.Log;
using Messages.User;
using UserStore.Consumers;

namespace UserStore;

public static class MassTransitSetup
{
    public static IServiceCollection AddUserServiceMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserNotificationConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"]);
                    h.Password(rabbit["Password"]);
                });

                var logExchange = rabbit["Exchanges:Log"];
                var userExchange = rabbit["Exchanges:User"];

                var userQueue = rabbit["Queues:User:Notifications"];
                var routingKeyRequest = rabbit["RoutingKeys:User:NotificationRequests"];

                cfg.Message<LogMessage>(m =>
                {
                    m.SetEntityName(logExchange);
                });
                cfg.Publish<LogMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                cfg.Message<UserNotificationMessage>(m =>
                {
                    m.SetEntityName(userExchange);
                });
                cfg.Publish<UserNotificationMessage>(m =>
                {
                    m.ExchangeType = ExchangeType.Topic;
                });

                cfg.ReceiveEndpoint(userQueue, e =>
                {
                    e.ConfigureConsumeTopology = false;

                    e.Bind(userExchange, s =>
                    {
                        s.RoutingKey = routingKeyRequest;
                        s.ExchangeType = ExchangeType.Topic;
                    });

                    e.ConfigureConsumer<UserNotificationConsumer>(context);
                    e.PrefetchCount = 10;
                    e.ConcurrentMessageLimit = 5;
                });
            });
        });

        return services;
    }
}
