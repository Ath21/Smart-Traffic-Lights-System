namespace LogMessages;

public record ErrorLogMessage(
    Guid LogId,
    string ServiceName,
    string ErrorType,
    string Message,
    DateTime Timestamp,
    object? Metadata
);