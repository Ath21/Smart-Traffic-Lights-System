namespace UserMessages;

// user.notification.request
public record UserNotificationRequest(
    Guid UserId,
    string RequestType,    // e.g. VerifyEmail, PasswordReset
    string TargetAudience,
    DateTime Timestamp
);