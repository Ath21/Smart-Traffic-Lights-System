using System;

namespace UserStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    public Task PublishNotificationAsync(Guid userId, string recipientEmail, string type, string message, string targetAudience);
}
