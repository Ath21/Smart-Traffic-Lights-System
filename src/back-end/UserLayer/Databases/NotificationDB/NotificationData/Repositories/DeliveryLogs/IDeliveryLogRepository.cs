using NotificationData.Collections;

namespace NotificationData.Repositories.DeliveryLogs;


public interface IDeliveryLogRepository
{
    Task<IEnumerable<DeliveryLogCollection>> GetByStatusAsync(string status);
    Task<IEnumerable<DeliveryLogCollection>> GetByNotificationAsync(string notificationId);
    Task InsertAsync(DeliveryLogCollection log);
}