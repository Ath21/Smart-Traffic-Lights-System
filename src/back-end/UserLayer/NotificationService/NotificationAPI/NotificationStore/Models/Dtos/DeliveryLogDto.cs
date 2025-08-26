namespace NotificationStore.Models.Dtos;

public class DeliveryLogDto
{
    public Guid DeliveryId { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
