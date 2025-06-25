/*
 * NotificationData.NotificationDbContext
 *
 * This class provides the context for accessing the MongoDB database used by the Notification Service.
 * It initializes the connection to the database and provides access to the notifications collection.
 * It uses the MongoDB.Driver package to interact with the database.
 */
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationData.Collections;

namespace NotificationData;

public class NotificationDbContext
{
    private readonly IMongoCollection<Notification> _notificationsCollection; 

    public NotificationDbContext(IOptions<NotificationDbSettings> notificationDbSettings)
    {
        var mongoClient = new MongoClient(
            notificationDbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            notificationDbSettings.Value.DatabaseName);

        _notificationsCollection = mongoDatabase.GetCollection<Notification>(
            notificationDbSettings.Value.NotificationsCollectionName);
    }

    public IMongoCollection<Notification> NotificationsCollection => _notificationsCollection;
}
