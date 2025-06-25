/*
 * UserMessages.LogInfo
 *
 * This file is part of the UserLayer project.
 * It defines a record type for logging informational messages in the system.
 * The LogInfo record contains properties for the message and the timestamp of when the message was logged.
 * It is used to track informational events for monitoring and debugging purposes.
 */
namespace UserMessages;

public record LogInfo(
    string Message,
    DateTime Timestamp);