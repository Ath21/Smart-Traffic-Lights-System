using NotificationStore.Models.Dtos;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    // [POST]   /api/notifications/send
    Task SendUserNotificationAsync(Guid userId, string email, string message, string type);
    Task SendNotificationAsync(NotificationDto notification);
    // [POST]   /api/notifications/public-notice
    Task SendPublicNoticeAsync(string title, string message, string audience);
    // [GET]   /api/notifications/recipient/{email}
    Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail);
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    // [GET]   /api/notifications/delivery-history/{userId}
    Task<IEnumerable<DeliveryLogDto>> GetDeliveryHistoryAsync(Guid userId);
    // [PATCH] /api/notifications/{notificationId}/read
    Task MarkAsReadAsync(Guid notificationId, string email);
    // [PATCH] /api/notifications/read-all
    Task MarkAllAsReadAsync(string email);
}
