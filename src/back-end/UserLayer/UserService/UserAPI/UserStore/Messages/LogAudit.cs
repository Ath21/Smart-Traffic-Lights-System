namespace UserStore.Messages;

public record class LogAudit
{
    Guid UserId { get; init; }
    string Action { get; init; }
    string Details { get; init; }
    DateTime Timestamp { get; init; }
}
