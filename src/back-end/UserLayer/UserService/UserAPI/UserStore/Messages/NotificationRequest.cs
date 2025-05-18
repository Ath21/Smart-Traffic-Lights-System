/*
 * UserStore.Messages.NotificationRequest
 *
 * This class represents a request for sending a notification in the UserStore application.
 * It contains properties for the user ID and the message to be sent.
 * The NotificationRequest class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
namespace UserStore.Messages;

public record class NotificationRequest(Guid UserId, string Message);
