namespace UserStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    Task PublishRequestAsync(string title, string body, string recipientEmail, string status = "Pending", Guid? correlationId = null);
}