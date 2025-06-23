/*
 * UserStore.Publishers.IUserLogPublisher
 * 
 * This interface defines the contract for publishing user-related logs in the UserStore application.
 * It includes methods for publishing informational logs, audit logs, and error logs.
 * The IUserLogPublisher interface is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 * Methods:
 *   - Task PublishInfoAsync(string message): Publishes an informational log message.
 *   - Task PublishAuditAsync(Guid userId, string action, string details): Publishes an audit log message
 *     for a specific user action.
 *   - Task PublishErrorAsync(string message, Exception exception): Publishes an error log message
 *     along with an exception.   
 *   - Task PublishNotificationAsync(Guid recipientId, string recipientEmail, string message, string type):   
 */
using UserMessages;

namespace UserStore.Publishers;

public interface IUserLogPublisher
{
    Task PublishInfoAsync(string message);
    Task PublishAuditAsync(Guid userId, string action, string details);
    Task PublishErrorAsync(string message, Exception exception);
    Task PublishNotificationAsync(Guid recipientId, string recipientEmail, string message, string type);
}
