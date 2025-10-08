using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Audit;

public class UserAuditRepository : BaseRepository<UserAuditEntity>, IUserAuditRepository
{
    public UserAuditRepository(UserDbContext context) : base(context) { }

    public async Task<IEnumerable<UserAuditEntity>> GetUserAuditsAsync(int userId)
        => await _context.UserAudits
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
}