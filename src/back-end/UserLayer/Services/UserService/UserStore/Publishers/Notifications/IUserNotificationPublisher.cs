namespace UserStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    Task PublishNotificationRequestAsync(
        string username,
        string status = "Pending",
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}