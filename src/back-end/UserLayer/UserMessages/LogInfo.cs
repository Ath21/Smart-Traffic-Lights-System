/*
 * UserMessages.LogInfo
 *
 * This class represents a message for logging informational messages in the UserStore application.
 * It contains properties for the message and the timestamp of the log entry.
 * The LogInfo class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
namespace UserMessages;

public record class LogInfo(string Message, DateTime Timestamp);