namespace UserStore.Messages;

public record class LogError(string ErrorMessage, DateTime utcNow, string StackTrace, DateTime Timestamp);