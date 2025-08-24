using System;

namespace NotificationStore.Publishers.Notifications;

public interface INotificationPublisher
{
    Task PublishPublicNoticeAsync(Guid noticeId, string title, string message, string targetAudience);
    Task PublishUserAlertAsync(Guid userId, string email, string alertType, string message);
}
