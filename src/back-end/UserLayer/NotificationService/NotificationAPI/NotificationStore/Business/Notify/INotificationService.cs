using System;
using NotificationStore.Models;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    Task CreateAsync(NotificationDto notification);
    Task<IEnumerable<NotificationDto>> GetAllAsync();
    Task<IEnumerable<NotificationDto>> GetByRecipientEmailAsync(string recipientEmail);
}
