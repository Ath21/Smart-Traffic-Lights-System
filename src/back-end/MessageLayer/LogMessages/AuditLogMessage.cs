namespace LogMessages;

// log.*.*.audit
public record AuditLogMessage(
    Guid LogId,
    string ServiceName,
    string Action,
    string Details,
    DateTime Timestamp,
    object? Metadata
);