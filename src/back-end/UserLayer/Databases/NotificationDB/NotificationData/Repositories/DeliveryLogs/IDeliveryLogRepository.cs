using NotificationData.Collections;

namespace NotificationData.Repositories.DeliveryLogs;

public interface IDeliveryLogRepository
{
    Task<IEnumerable<DeliveryLogCollection>> GetByStatusAsync(string status);
    Task<IEnumerable<DeliveryLogCollection>> GetByNotificationAsync(string notificationId);
    Task<IEnumerable<DeliveryLogCollection>> GetByRecipientEmailAsync(string email);

    Task InsertAsync(DeliveryLogCollection log);

    Task MarkAsReadAsync(string notificationId, string email);
    Task MarkAllAsReadAsync(string email);
}
