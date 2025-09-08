using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Ses;

public class SessionRepository : ISessionRepository
{
    private readonly UserDbContext _dbContext;

    public SessionRepository(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Session session)
    {
        await _dbContext.Sessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteByTokenAsync(string token)
    {
        var session = await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Token == token);
        if (session != null)
        {
            _dbContext.Sessions.Remove(session);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var sessions = await _dbContext.Sessions.Where(s => s.UserId == userId).ToListAsync();
        if (sessions.Any())
        {
            _dbContext.Sessions.RemoveRange(sessions);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<Session?> GetByTokenAsync(string token)
    {
        return await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Token == token);
    }
}
