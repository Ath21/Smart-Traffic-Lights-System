namespace TrafficMessages.Logs;

public record ErrorLogMessage(
    string Service,
    string Message,
    string Exception,
    DateTime Timestamp
);