namespace NotificationStore.Models;

public class DeliveryLogDto
{
    public Guid DeliveryId { get; set; }
    public Guid NotificationId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
