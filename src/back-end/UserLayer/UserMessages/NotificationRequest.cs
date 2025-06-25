/*
 * UserMessages.NotificationRequest
 *
 * This file is part of the UserLayer project.
 * It defines a record type for notification requests in the system.
 * The NotificationRequest record contains properties for the recipient's ID, email, message content,
 * type of notification, and the timestamp of when the notification was created.
 * It is used to send notifications to users in the system.
 */
namespace UserMessages;

public record NotificationRequest(
    Guid RecipientId,
    string RecipientEmail,
    string Message,
    string Type,
    DateTime Timestamp);