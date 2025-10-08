using UserData.Entities;

namespace UserData.Repositories.Ses;

public interface ISessionRepository
{
    Task<IEnumerable<SessionEntity>> GetUserSessionsAsync(int userId);
    Task<SessionEntity?> GetActiveSessionAsync(int userId);
    Task InsertAsync(SessionEntity session);
    Task EndSessionAsync(int sessionId);
}