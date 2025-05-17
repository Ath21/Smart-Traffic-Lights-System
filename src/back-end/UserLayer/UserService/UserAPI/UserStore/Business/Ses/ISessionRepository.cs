using System;
using UserData.Entities;

namespace UserStore.Business.Ses;

public interface ISessionRepository
{
    Task CreateSessionAsync(Session session);
    Task<Session> GetSessionByTokenAsync(string token);
    Task DeleteSessionAsync(string token);
    Task DeleteSessionsByUserIdAsync(Guid userId);
}
