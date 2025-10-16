using System;

namespace NotificationStore.Models.Dtos;

public class NotificationDto
{
    public string NotificationId { get; set; } = string.Empty;

    // Alert, PublicNotice, Private, Request
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Recipient email or audience identifier
    public string RecipientEmail { get; set; } = string.Empty;

    // Status: Pending, Sent, Broadcasted, Delivered, etc.
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Read tracking (maps to DeliveryLogCollection)
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
