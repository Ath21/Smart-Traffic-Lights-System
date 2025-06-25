/*
 * NotificationStore.Repository.INotificationRepository
 *
 * This file is part of the NotificationStore project, which defines the interface for notification repository operations.
 * The INotificationRepository interface declares methods for creating notifications,
 * retrieving all notifications, and getting notifications by recipient email.
 * It is used to abstract the data access layer for notifications, allowing for different implementations
 * (e.g., in-memory, database) without changing the service layer.
 * The interface is typically implemented by a class that interacts with a database or other storage mechanism.
 */
using NotificationData.Collections;

namespace NotificationStore.Repository;

public interface INotificationRepository
{
    Task CreateAsync(Notification newNotification);
    Task<List<Notification>> GetAllAsync();
    Task<IEnumerable<Notification>> GetByRecipientEmailAsync(string recipientEmail);


}
