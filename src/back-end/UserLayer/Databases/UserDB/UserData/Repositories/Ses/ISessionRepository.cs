using UserData.Entities;

namespace UserData.Repositories.Ses;

public interface ISessionRepository
{
    Task AddAsync(Session session);
    Task<Session?> GetByTokenAsync(string token);
    Task DeleteByTokenAsync(string token);
    Task DeleteByUserIdAsync(Guid userId);
}
