namespace NotificationStore.Publishers.Notifications;

public interface INotificationPublisher
{
    // notification.event.public_notice
    Task PublishPublicNoticeAsync(Guid noticeId, string title, string message, string targetAudience);
    // user.notification.alert
    Task PublishUserAlertAsync(Guid userId, string email, string alertType, string message);
}
