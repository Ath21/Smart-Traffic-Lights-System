using MongoDB.Driver;
using NotificationData;
using NotificationData.Collections;

namespace NotificationData.Repositories.DeliveryLogs;

public class DeliveryLogRepository : BaseRepository<DeliveryLogCollection>, IDeliveryLogRepository
{
    public DeliveryLogRepository(IMongoCollection<DeliveryLogCollection> collection) : base(collection) { }

    public async Task<IEnumerable<DeliveryLogCollection>> GetByStatusAsync(string status)
        => await _collection.Find(l => l.Status == status).SortByDescending(l => l.DeliveredAt).ToListAsync();

    public async Task<IEnumerable<DeliveryLogCollection>> GetByNotificationAsync(string notificationId)
        => await _collection.Find(l => l.NotificationId == notificationId).ToListAsync();

    public async Task InsertAsync(DeliveryLogCollection log)
        => await _collection.InsertOneAsync(log);
}