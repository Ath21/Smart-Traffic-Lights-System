namespace NotificationStore.Models.Responses;


public class NotificationResponse
{
    public string NotificationId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; } = null;
}
