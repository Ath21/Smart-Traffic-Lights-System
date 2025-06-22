using System;
using NotificationData.Collections;

namespace NotificationStore.Repository;

public interface INotificationRepository
{
    Task CreateAsync(Notification newNotification);
    Task<List<Notification>> GetAllAsync();
    Task<List<Notification?>> GetAsync(Guid Id);
}
