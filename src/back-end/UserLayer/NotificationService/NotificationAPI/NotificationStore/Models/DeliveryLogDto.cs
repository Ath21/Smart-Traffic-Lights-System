namespace NotificationStore.Models;

public class DeliveryLogDto
{
    public Guid DeliveryId { get; set; }
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }            // ✅ added
    public string RecipientEmail { get; set; } = string.Empty; // ✅ added

    public string Status { get; set; } = "Pending";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
