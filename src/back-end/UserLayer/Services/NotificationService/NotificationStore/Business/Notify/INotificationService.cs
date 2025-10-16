using NotificationStore.Models.Dtos;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    Task SendUserNotificationAsync(int userId, string email, string message, string type);
    Task SendPublicNoticeAsync(string title, string message, string audience);
    Task SendNotificationAsync(NotificationDto notification);
    Task<IEnumerable<NotificationDto>> GetPublicNoticesAsync();
    Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail);
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    Task MarkAsReadAsync(string notificationId, string email);
    Task MarkAllAsReadAsync(string email);
}
