using System;

namespace UserStore.Publishers.Notifications;

public interface IUserNotificationPublisher
{
    Task PublishNotificationAsync(Guid userId, string requestType, string targetAudience);
}
