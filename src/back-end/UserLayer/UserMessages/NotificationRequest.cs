namespace UserMessages;

public record NotificationRequest(
    Guid UserId,
    string Message);