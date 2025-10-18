using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using NotificationData.Collections;

namespace NotificationData.Repositories.DeliveryLogs;

public class DeliveryLogRepository : IDeliveryLogRepository
{
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<DeliveryLogRepository> _logger;

    public DeliveryLogRepository(NotificationDbContext dbContext, ILogger<DeliveryLogRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogDeliveryAsync(string userId, string userEmail, string messageId, string status)
    {
        _logger.LogInformation("[REPOSITORY][DELIVERYLOG] Logging {Status} delivery for {UserId} ({UserEmail}) ({MessageId})", status, userId, userEmail, messageId);

        var log = new DeliveryLogCollection
        {
            UserId = userId,
            UserEmail = userEmail,
            MessageId = messageId,
            DeliveredAt = DateTime.UtcNow,
            Status = status
        };

        await _dbContext.DeliveryLogs.InsertOneAsync(log);
    }

        public async Task<IEnumerable<DeliveryLogCollection>> GetUserDeliveriesAsync(string userId, bool unreadOnly = false)
    {
        _logger.LogInformation("[REPOSITORY][DELIVERY] Fetching deliveries for {UserId}", userId);

        var filter = Builders<DeliveryLogCollection>.Filter.Eq(l => l.UserId, userId);

        if (unreadOnly)
        {
            filter = Builders<DeliveryLogCollection>.Filter.And(
                filter,
                Builders<DeliveryLogCollection>.Filter.Ne(l => l.Status, "Read")
            );
        }

        return await _dbContext.DeliveryLogs.Find(filter)
            .SortByDescending(l => l.DeliveredAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(string userId, string userEmail, string deliveryId)
    {
        _logger.LogInformation("[REPOSITORY][DELIVERY] Marking delivery {DeliveryId} as read for {UserId}/{UserEmail}", deliveryId, userId, userEmail);

        var filter = Builders<DeliveryLogCollection>.Filter.And(
            Builders<DeliveryLogCollection>.Filter.Eq(l => l.Id, deliveryId),
            Builders<DeliveryLogCollection>.Filter.Eq(l => l.UserId, userId),
            Builders<DeliveryLogCollection>.Filter.Eq(l => l.UserEmail, userEmail)
        );

        var update = Builders<DeliveryLogCollection>.Update
            .Set(l => l.Status, "Read");

        await _dbContext.DeliveryLogs.UpdateOneAsync(filter, update);
    }
}
