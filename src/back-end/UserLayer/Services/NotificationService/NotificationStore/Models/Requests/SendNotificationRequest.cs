namespace NotificationStore.Models.Requests;

public class SendNotificationRequest
{
    public int UserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
