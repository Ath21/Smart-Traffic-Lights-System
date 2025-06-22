using System;

namespace NotificationStore.Models;

public class NotificationDto
{
    public string Type { get; set; }
    public string RecipientId { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
