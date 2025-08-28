using MongoDB.Driver;
using NotificationData;
using NotificationData.Collections;

namespace NotificationStore.Repositories.DeliveryLogs;

public class DeliveryLogRepository : IDeliveryLogRepository
{
    private readonly NotificationDbContext _context;

    public DeliveryLogRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(DeliveryLog newLog) =>
        await _context.DeliveryLogs.InsertOneAsync(newLog);

    public async Task<List<DeliveryLog>> GetByNotificationIdAsync(Guid notificationId) =>
        await _context.DeliveryLogs.Find(l => l.NotificationId == notificationId).ToListAsync();

    public async Task<IEnumerable<DeliveryLog>> GetByRecipientEmailAsync(string email) =>
        await _context.DeliveryLogs.Find(l => l.RecipientEmail == email).ToListAsync();


    public async Task<IEnumerable<DeliveryLog>> GetByUserIdAsync(Guid userId) =>
        await _context.DeliveryLogs.Find(l => l.UserId == userId).ToListAsync();

    public async Task MarkAsReadAsync(Guid notificationId, string email)
    {
        var filter = Builders<DeliveryLog>.Filter.Where(
            l => l.NotificationId == notificationId && l.RecipientEmail == email
        );
        var update = Builders<DeliveryLog>.Update.Set(l => l.IsRead, true);
        await _context.DeliveryLogs.UpdateOneAsync(filter, update);
    }

    public async Task MarkAllAsReadAsync(string email)
    {
        var filter = Builders<DeliveryLog>.Filter.Eq(l => l.RecipientEmail, email);
        var update = Builders<DeliveryLog>.Update.Set(l => l.IsRead, true);
        await _context.DeliveryLogs.UpdateManyAsync(filter, update);
    }

}
