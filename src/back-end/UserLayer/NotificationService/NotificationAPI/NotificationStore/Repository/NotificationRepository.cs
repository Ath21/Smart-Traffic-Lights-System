using System;
using MongoDB.Driver;
using NotificationData;
using NotificationData.Collections;

namespace NotificationStore.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context; 
    }

    public async Task CreateAsync(Notification newNotification)
    {
        await _context.NotificationsCollection.InsertOneAsync(newNotification);
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await _context.NotificationsCollection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByRecipientEmailAsync(string recipientEmail)
    {
        return await _context.NotificationsCollection.Find(n => n.RecipientEmail == recipientEmail).ToListAsync();
    }
}
