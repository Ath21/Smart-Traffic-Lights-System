using System;
using NotificationData.Collections;

namespace NotificationStore.Repositories.DeliveryLogs;

public interface IDeliveryLogRepository
{
    Task InsertAsync(DeliveryLog newLog);
    Task<List<DeliveryLog>> GetByNotificationIdAsync(Guid notificationId);
    Task<IEnumerable<DeliveryLog>> GetByRecipientAsync(string recipient);
}