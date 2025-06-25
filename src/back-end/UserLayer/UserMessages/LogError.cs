/*
 * UserMessages.LogError
 *
 * This file is part of the UserLayer project.
 * It defines a record type for logging errors in the system.
 * The LogError record contains properties for the error message, stack trace, and timestamp of when the error occurred.
 * It is used to track errors for debugging and monitoring purposes.
 */
namespace UserMessages;

public record LogError(
    string ErrorMessage,
    string StackTrace,
    DateTime Timestamp);