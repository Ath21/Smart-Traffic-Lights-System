namespace UserStore.Messages;

public record class LogError
{
    string ErrorMessage { get; init; }
    string StackTrace { get; init; }
    DateTime Timestamp { get; init; }
}
