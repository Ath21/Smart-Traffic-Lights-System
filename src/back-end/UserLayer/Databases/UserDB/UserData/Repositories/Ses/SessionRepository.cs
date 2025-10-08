using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Ses;

public class SessionRepository : BaseRepository<SessionEntity>, ISessionRepository
{
    public SessionRepository(UserDbContext context) : base(context) { }

    public async Task<IEnumerable<SessionEntity>> GetUserSessionsAsync(int userId)
        => await _context.Sessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginTime)
            .ToListAsync();

    public async Task<SessionEntity?> GetActiveSessionAsync(int userId)
        => await _context.Sessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

    public async Task EndSessionAsync(int sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.IsActive = false;
            session.LogoutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
