namespace LogStore.Messages;

public record class LogMessage(
    string LogLevel,
    string Service,
    string Message,
    DateTime Timestamp
);
