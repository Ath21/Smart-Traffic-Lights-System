namespace LogStore.Messages;

public record class TrafficLogMessage(
    string IntersectionId,
    string Message,
    string Source,
    DateTime Timestamp
);