/*
 * NotificationData.NotificationDbSettings
 *
 * This class holds the settings for connecting to the MongoDB database used by the Notification Service.
 * It includes properties for the connection string, database name, and collection name.
 * It is used to configure the database context for accessing notification data.
 */
namespace NotificationData;

public class NotificationDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string NotificationsCollectionName { get; set; } = null!;
}
