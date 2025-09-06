namespace NotificationStore.Models.Responses;

public class DeliveryLogResponse
{
    public Guid DeliveryId { get; set; }
    public Guid NotificationId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}
