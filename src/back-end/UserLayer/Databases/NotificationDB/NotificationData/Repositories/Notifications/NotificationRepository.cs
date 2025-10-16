using MongoDB.Driver;
using NotificationData;
using NotificationData.Collections;

namespace NotificationData.Repositories.Notifications;

public class NotificationRepository : BaseRepository<NotificationCollection>, INotificationRepository
{
    public NotificationRepository(NotificationDbContext context)
        : base(context.Notifications) // use collection from context
    {
    }

    public async Task<IEnumerable<NotificationCollection>> GetPendingAsync()
        => await _collection.Find(n => n.Status == "Pending")
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<NotificationCollection>> GetBroadcastedAsync()
        => await _collection.Find(n => n.Status == "Broadcasted" || n.Type == "public-notice")
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<NotificationCollection?> GetByIdAsync(string id)
        => await _collection.Find(n => n.NotificationId == id).FirstOrDefaultAsync();

    public async Task InsertAsync(NotificationCollection notification)
        => await _collection.InsertOneAsync(notification);

    public async Task UpdateStatusAsync(string notificationId, string newStatus)
    {
        var filter = Builders<NotificationCollection>.Filter.Eq(n => n.NotificationId, notificationId);
        var update = Builders<NotificationCollection>.Update.Set(n => n.Status, newStatus);
        await _collection.UpdateOneAsync(filter, update);
    }
}
