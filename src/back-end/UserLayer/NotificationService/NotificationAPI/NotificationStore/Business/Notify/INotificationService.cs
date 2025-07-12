/*
 * NotificationStore.Business.Notify.INotificationService
 * 
 * This file is part of the NotificationStore project, which defines the INotificationService interface.
 * The INotificationService interface declares methods for creating notifications,
 * retrieving all notifications, and getting notifications by recipient email.
 * It is used to abstract the business logic layer for notifications, allowing for different implementations
 * (e.g., in-memory, database) without changing the API layer.
 * The interface is typically implemented by a class that interacts with a repository or data access layer.
 */
using NotificationStore.Models;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationDto notification);
    Task CreateAsync(NotificationDto notification);
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail);
}
