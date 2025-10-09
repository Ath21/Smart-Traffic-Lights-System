namespace NotificationStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    Task PublishAlertAsync(string title, string body, string recipientEmail = "all@uniwa-stls", string status = "Broadcasted", int intersectionId = 0, string? intersectionName = null, Guid? correlationId = null);
    Task PublishPublicNoticeAsync(string title, string body, string recipientEmail = "all@uniwa-stls", string status = "Broadcasted", Guid? correlationId = null);
    Task PublishPrivateAsync(string title, string body, string recipientEmail, string status = "Sent", int intersectionId = 0, string? intersectionName = null, Guid? correlationId = null);
    Task PublishRequestAsync(string title, string body, string recipientEmail, string status = "Pending", Guid? correlationId = null);
}