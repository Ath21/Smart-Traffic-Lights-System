using System;
using NotificationStore.Models;

namespace NotificationStore.Business.Notify;

public interface INotificationService
{
    Task<List<NotificationDto?>> GetByRecipientAsync(Guid recipientId);
    Task CreateAsync(NotificationDto notification);
}
