namespace TrafficMessages.Logs;

public record AuditLogMessage(
    string Service,
    string Message,
    DateTime Timestamp
);
