namespace NotificationStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    Task PublishUserAlertAsync(int userId, string recipientEmail, string type, string message);
    Task PublishPublicNoticeAsync(string notificationId, string title, string body, string audience);
}