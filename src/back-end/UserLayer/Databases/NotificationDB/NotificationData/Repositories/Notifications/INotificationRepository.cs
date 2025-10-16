using NotificationData.Collections;

namespace NotificationData.Repositories.Notifications;


public interface INotificationRepository
{
    Task<IEnumerable<NotificationCollection>> GetPendingAsync();
    Task<IEnumerable<NotificationCollection>> GetBroadcastedAsync();
    Task<NotificationCollection?> GetByIdAsync(string id);
    Task InsertAsync(NotificationCollection notification);
    Task UpdateStatusAsync(string notificationId, string newStatus);
}