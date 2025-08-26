using NotificationStore.Models.Dtos;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    // Legacy
    Task SendNotificationAsync(NotificationDto notification);

    // Queries
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail);
    Task<IEnumerable<DeliveryLogDto>> GetDeliveryHistoryAsync(Guid userId);

    // Mutations
    Task SendUserNotificationAsync(Guid userId, string email, string message, string type);
    Task SendPublicNoticeAsync(string title, string message, string audience);
    Task MarkAsReadAsync(Guid notificationId, string email);
    Task MarkAllAsReadAsync(string email);
}
