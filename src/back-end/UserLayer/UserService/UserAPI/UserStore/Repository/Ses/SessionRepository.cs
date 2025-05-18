/*
 * UserStore.Repository.Ses.SessionRepository
 *
 * This class implements the ISessionRepository interface and provides methods for managing Session entities.
 * It uses Entity Framework Core to interact with the database.
 * The methods include:
 * - CreateSessionAsync: Creates a new Session entity in the database.
 * - GetSessionByTokenAsync: Retrieves a Session entity by its unique token.
 * - DeleteSessionAsync: Deletes a Session entity by its unique token.
 * - DeleteSessionsByUserIdAsync: Deletes all Session entities associated with a specific UserId.
 * The class uses the UserDbContext to interact with the database and perform the necessary operations.
 * The SessionRepository class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using UserData;
using UserData.Entities;

namespace UserStore.Repository.Ses;

public class SessionRepository : ISessionRepository
{
    private readonly UserDbContext _dbContext;

    public SessionRepository(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task CreateSessionAsync(Session session)
    {
        _dbContext.Sessions.Add(session);
        return _dbContext.SaveChangesAsync();
    }

    public Task DeleteSessionAsync(string token)
    {
        var session = _dbContext.Sessions.FirstOrDefault(s => s.Token == token);
        if (session != null)
        {
            _dbContext.Sessions.Remove(session);
            return _dbContext.SaveChangesAsync();
        }

        return Task.CompletedTask;
    }

    public Task DeleteSessionsByUserIdAsync(Guid userId)
    {
        var sessions = _dbContext.Sessions.Where(s => s.UserId == userId).ToList();
        if (sessions.Any())
        {
            _dbContext.Sessions.RemoveRange(sessions);
            return _dbContext.SaveChangesAsync();
        }

        return Task.CompletedTask;
    }

    public Task<Session> GetSessionByTokenAsync(string token)
    {
        var session = _dbContext.Sessions.FirstOrDefault(s => s.Token == token);
        return Task.FromResult(session);
    }
}
