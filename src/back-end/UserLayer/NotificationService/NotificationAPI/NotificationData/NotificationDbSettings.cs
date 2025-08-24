namespace NotificationData;

public class NotificationDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;

    // Collection names
    public string NotificationsCollectionName { get; set; } = "notifications";
    public string DeliveryLogsCollectionName { get; set; } = "delivery_logs";
}
