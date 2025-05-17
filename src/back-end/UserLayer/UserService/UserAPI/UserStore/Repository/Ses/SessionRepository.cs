using System;
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
