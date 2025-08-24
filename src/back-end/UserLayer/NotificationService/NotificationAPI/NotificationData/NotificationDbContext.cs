using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationData.Collections;

namespace NotificationData;

public class NotificationDbContext
{
    private readonly IMongoDatabase _database;

    public NotificationDbContext(IOptions<NotificationDbSettings> notificationDbSettings)
    {
        var mongoClient = new MongoClient(notificationDbSettings.Value.ConnectionString);
        _database = mongoClient.GetDatabase(notificationDbSettings.Value.DatabaseName);

        Notifications = _database.GetCollection<Notification>(notificationDbSettings.Value.NotificationsCollectionName);
        DeliveryLogs = _database.GetCollection<DeliveryLog>(notificationDbSettings.Value.DeliveryLogsCollectionName);
    }

    // Exposed collections
    public IMongoCollection<Notification> Notifications { get; }
    public IMongoCollection<DeliveryLog> DeliveryLogs { get; }
}
