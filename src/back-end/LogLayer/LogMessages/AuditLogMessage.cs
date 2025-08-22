namespace LogMessages;

public record AuditLogMessage
(
    Guid LogId,
    string ServiceName,
    string Action,
    string Message,
    DateTime Timestamp,
    object? Metadata
);