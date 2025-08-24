namespace NotificationStore.Models;

public class SendNotificationRequest
{
    public Guid UserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
