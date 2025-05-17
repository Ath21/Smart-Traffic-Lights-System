namespace UserStore.Messages;

public record class NotificationRequest
{
    Guid UserId { get; init; }
    string Message { get; init; }
}