namespace NotificationStore.Models.Responses;

public class NotificationResponse
{
    public Guid NotificationId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string RecipientEmail { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
