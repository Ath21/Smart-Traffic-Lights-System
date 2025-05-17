namespace UserStore.Messages;

public record class LogInfo
{
    string Message { get; init; }
    DateTime Timestamp { get; init; }
}
