/*
 * UserMessages.LogAudit
 *
 * This file is part of the UserLayer project.
 * It defines a record type for logging user actions in the system.
 * The LogAudit record contains properties for user ID, action performed, details of the action,
 * and the timestamp of when the action occurred.
 * It is used to track user activities for auditing purposes.
 */
namespace UserMessages;

public record LogAudit(
    Guid UserId,
    string Action,
    string Details,
    DateTime Timestamp);