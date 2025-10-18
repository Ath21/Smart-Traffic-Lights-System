using Messages.User;

namespace NotificationStore.Publishers.Notifications;

public interface INotificationPublisher
{
    Task PublishNotificationAsync(UserNotificationMessage message, string routingKey);
}