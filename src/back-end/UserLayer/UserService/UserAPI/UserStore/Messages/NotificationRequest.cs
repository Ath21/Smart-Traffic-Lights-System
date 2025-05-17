namespace UserStore.Messages;

public record class NotificationRequest(Guid UserId, string Message);
