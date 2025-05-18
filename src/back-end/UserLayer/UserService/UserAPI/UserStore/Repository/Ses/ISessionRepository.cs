/*
 * UserStore.Repository.Ses.ISessionRepository
 *
 * This interface defines the contract for the Session repository in the UserStore application.
 * It contains methods for performing CRUD operations on Session entities.
 * The methods include:
 * - CreateSessionAsync: Creates a new Session entity in the database.
 * - GetSessionByTokenAsync: Retrieves a Session entity by its unique token.
 * - DeleteSessionAsync: Deletes a Session entity by its unique token.
 * - DeleteSessionsByUserIdAsync: Deletes all Session entities associated with a specific UserId.
 * The ISessionRepository interface is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using UserData.Entities;

namespace UserStore.Repository.Ses;

public interface ISessionRepository
{
    Task CreateSessionAsync(Session session);
    Task<Session> GetSessionByTokenAsync(string token);
    Task DeleteSessionAsync(string token);
    Task DeleteSessionsByUserIdAsync(Guid userId);
}
