/*
 * NotificationStore.Repository.NotificationRepository
 *
 * This file is part of the NotificationStore project, which implements the INotificationRepository interface.
 * The NotificationRepository class provides methods for creating notifications,
 * retrieving all notifications, and getting notifications by recipient email.
 * It uses MongoDB as the data store and interacts with the NotificationDbContext to perform database operations.
 * The class is responsible for encapsulating the data access logic related to notifications,
 * allowing for separation of concerns between the business logic and data access layers.
 */
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

    public async Task InsertAsync(Notification newNotification)
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
