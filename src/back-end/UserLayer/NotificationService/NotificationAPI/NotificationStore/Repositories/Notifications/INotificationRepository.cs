using System;
using NotificationData.Collections;

namespace NotificationStore.Repositories.Notifications;

public interface INotificationRepository
{
    Task InsertAsync(Notification newNotification);
    Task<List<Notification>> GetAllAsync();
    Task<Notification?> GetByIdAsync(Guid notificationId);
    Task UpdateStatusAsync(Guid notificationId, string status);
    Task<IEnumerable<Notification>> GetByRecipientEmailAsync(string recipientEmail);
}
