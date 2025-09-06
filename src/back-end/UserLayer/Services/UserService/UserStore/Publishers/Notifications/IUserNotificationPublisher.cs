namespace UserStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    // user.notification.request
    public Task PublishNotificationAsync(Guid userId, string recipientEmail, string type, string message, string targetAudience);
}
