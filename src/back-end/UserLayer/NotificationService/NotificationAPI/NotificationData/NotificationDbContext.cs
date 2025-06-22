using System;
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
