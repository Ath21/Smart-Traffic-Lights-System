namespace UserMessages;

// user.notification.alert
public record UserNotificationAlert(
    Guid UserId,
    string Email,
    string AlertType,
    string Message,
    DateTime Timestamp
);