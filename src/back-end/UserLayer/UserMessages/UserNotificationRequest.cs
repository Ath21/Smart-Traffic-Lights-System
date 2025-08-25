namespace UserMessages;

// user.notification.request
public record UserNotificationRequest(
    Guid UserId,
    string RecipientEmail,   // actual user email
    string Type,             // e.g. Welcome, Alert
    string Message,          // notification text
    string TargetAudience,   // optional, e.g. "User", "Admins"
    DateTime Timestamp
);
