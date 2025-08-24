using System;
using MongoDB.Driver;
using NotificationData;
using NotificationData.Collections;

namespace NotificationStore.Repositories.Notifications;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(Notification newNotification) =>
        await _context.Notifications.InsertOneAsync(newNotification);

    public async Task<List<Notification>> GetAllAsync() =>
        await _context.Notifications.Find(_ => true).ToListAsync();

    public async Task<Notification?> GetByIdAsync(Guid notificationId) =>
        await _context.Notifications.Find(n => n.NotificationId == notificationId).FirstOrDefaultAsync();

    public async Task UpdateStatusAsync(Guid notificationId, string status)
    {
        var update = Builders<Notification>.Update.Set(n => n.Status, status);
        await _context.Notifications.UpdateOneAsync(n => n.NotificationId == notificationId, update);
    }
}
