namespace UserMessages;

// user.notification.request
public record UserNotificationRequest(
    Guid UserId,
    string RecipientEmail,   
    string Type,           
    string Message,          
    string TargetAudience,   
    DateTime Timestamp
);
