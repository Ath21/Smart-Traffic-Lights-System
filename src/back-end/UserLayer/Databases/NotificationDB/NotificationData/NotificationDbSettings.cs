namespace NotificationData;

public class NotificationDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string Database { get; set; } = null!;
    public string NotificationsCollection { get; set; } = "notifications";
    public string DeliveryLogsCollection { get; set; } = "delivery_logs";
}
