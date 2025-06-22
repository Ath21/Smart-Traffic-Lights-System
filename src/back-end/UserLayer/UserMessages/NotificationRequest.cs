namespace UserMessages;

public record NotificationRequest(
    Guid UserId,
    string Email,
    string Message);