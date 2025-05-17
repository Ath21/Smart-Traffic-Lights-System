namespace UserStore.Messages;

public record class LogAudit(Guid UserId, string Action, string Details, DateTime Timestamp);