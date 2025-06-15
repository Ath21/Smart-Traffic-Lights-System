namespace UserMessages;

public record LogAudit(
    Guid UserId,
    string Action,
    string Details,
    DateTime Timestamp);