using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Ses;


public class SessionRepository : ISessionRepository
{
    private readonly UserDbContext _context;

    public SessionRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SessionEntity>> GetUserSessionsAsync(int userId)
        => await _context.Sessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginTime)
            .ToListAsync();

    public async Task<SessionEntity?> GetActiveSessionAsync(int userId)
        => await _context.Sessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

    public async Task<SessionEntity?> GetByTokenAsync(string token)
        => await _context.Sessions.FirstOrDefaultAsync(s => s.Session == token);

    public async Task InsertAsync(SessionEntity session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SessionEntity session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SessionEntity session)
    {
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
    }
}
