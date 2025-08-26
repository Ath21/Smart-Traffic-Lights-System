using NotificationStore.Models;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    Task SendNotificationAsync(NotificationDto notification);
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail);

    // New API-driven methods
    Task SendUserNotificationAsync(Guid userId, string email, string message, string type);
    Task SendPublicNoticeAsync(string title, string message, string audience);
    Task<IEnumerable<DeliveryLogDto>> GetDeliveryHistoryAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, string email);
    Task MarkAllAsReadAsync(string email);
}
