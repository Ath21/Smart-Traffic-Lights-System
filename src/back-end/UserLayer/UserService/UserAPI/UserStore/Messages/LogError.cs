/*
 * UserStore.Messages.LogError
 *
 * This class represents a message for logging error information in the UserStore application.
 * It contains properties for the error message, stack trace, and timestamp of the error.
 * The LogError class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
namespace UserStore.Messages;

public record class LogError(
    string ErrorMessage,
    string StackTrace,
    DateTime Timestamp);