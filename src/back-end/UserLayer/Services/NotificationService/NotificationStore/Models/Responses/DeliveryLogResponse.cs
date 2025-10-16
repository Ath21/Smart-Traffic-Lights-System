namespace NotificationStore.Models.Responses;


public class DeliveryLogResponse
{
    public string DeliveryId { get; set; } = string.Empty;
    public string NotificationId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string DeliveryMethod { get; set; } = "Email";
    public DateTime DeliveredAt { get; set; }
}