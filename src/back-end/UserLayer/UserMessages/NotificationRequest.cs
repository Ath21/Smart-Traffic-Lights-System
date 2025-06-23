namespace UserMessages;

public record NotificationRequest(
    Guid RecipientId,
    string RecipientEmail,
    string Message,
    string Type,
    DateTime Timestamp);