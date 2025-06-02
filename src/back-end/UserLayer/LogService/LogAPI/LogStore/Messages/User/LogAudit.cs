/*
 * LogStore.Messages.LogAudit
 *
 * This class represents a message for logging audit information in the UserStore application.
 * It contains properties for the user ID, action performed, details of the action, and the timestamp of the action.
 * The LogAudit class is typically used in the UserService layer of the application.
 * It is part of the LogStore project, which is responsible for managing user-related operations
 * and services.
 */
namespace LogStore.Messages.User;

public record class LogAudit(Guid UserId, string Action, string Details, DateTime Timestamp);