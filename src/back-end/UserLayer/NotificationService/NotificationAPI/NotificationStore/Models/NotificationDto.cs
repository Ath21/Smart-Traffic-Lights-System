/*
 * NotificationStore.Models.NotificationDto
 * This file is part of the NotificationStore project, which defines the NotificationDto model.
 * The NotificationDto class contains properties for notification details,
 * including type, recipient information, message content, and timestamp.
 * It is used to represent notifications in the system.
 */
namespace NotificationStore.Models;

public class NotificationDto
{
    public string Type { get; set; }
    public Guid RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
