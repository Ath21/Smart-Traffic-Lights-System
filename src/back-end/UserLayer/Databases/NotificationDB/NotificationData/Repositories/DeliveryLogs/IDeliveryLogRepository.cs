using NotificationData.Collections;

namespace NotificationData.Repositories.DeliveryLogs;

public interface IDeliveryLogRepository
{
    Task InsertAsync(DeliveryLog newLog);
    Task<List<DeliveryLog>> GetByNotificationIdAsync(Guid notificationId);
    Task<IEnumerable<DeliveryLog>> GetByRecipientEmailAsync(string email);
    Task<IEnumerable<DeliveryLog>> GetByUserIdAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, string email);
    Task MarkAllAsReadAsync(string email);
    Task<IEnumerable<Guid>> GetBroadcastedNotificationIdsAsync();
}