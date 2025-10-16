using MongoDB.Driver;
using NotificationData;
using NotificationData.Collections;

namespace NotificationData.Repositories.DeliveryLogs;

public class DeliveryLogRepository : BaseRepository<DeliveryLogCollection>, IDeliveryLogRepository
{
    public DeliveryLogRepository(NotificationDbContext context)
        : base(context.DeliveryLogs)
    {
    }

    public async Task<IEnumerable<DeliveryLogCollection>> GetByStatusAsync(string status)
        => await _collection.Find(l => l.Status == status)
            .SortByDescending(l => l.DeliveredAt)
            .ToListAsync();

    public async Task<IEnumerable<DeliveryLogCollection>> GetByNotificationAsync(string notificationId)
        => await _collection.Find(l => l.NotificationId == notificationId).ToListAsync();

    public async Task<IEnumerable<DeliveryLogCollection>> GetByRecipientEmailAsync(string email)
        => await _collection.Find(l => l.RecipientEmail == email)
            .SortByDescending(l => l.DeliveredAt)
            .ToListAsync();

    public async Task InsertAsync(DeliveryLogCollection log)
        => await _collection.InsertOneAsync(log);

    public async Task MarkAsReadAsync(string notificationId, string email)
    {
        var filter = Builders<DeliveryLogCollection>.Filter.And(
            Builders<DeliveryLogCollection>.Filter.Eq(l => l.NotificationId, notificationId),
            Builders<DeliveryLogCollection>.Filter.Eq(l => l.RecipientEmail, email)
        );

        var update = Builders<DeliveryLogCollection>.Update
            .Set(l => l.IsRead, true)
            .Set(l => l.ReadAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task MarkAllAsReadAsync(string email)
    {
        var filter = Builders<DeliveryLogCollection>.Filter.Eq(l => l.RecipientEmail, email);
        var update = Builders<DeliveryLogCollection>.Update
            .Set(l => l.IsRead, true)
            .Set(l => l.ReadAt, DateTime.UtcNow);

        await _collection.UpdateManyAsync(filter, update);
    }
}
