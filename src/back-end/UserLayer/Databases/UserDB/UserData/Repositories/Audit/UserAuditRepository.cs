using Microsoft.EntityFrameworkCore;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Audit;

public class UserAuditRepository : IUserAuditRepository
{
    private readonly UserDbContext _context;

    public UserAuditRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAuditEntity>> GetUserAuditsAsync(int userId)
        => await _context.UserAudits
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task InsertAsync(UserAuditEntity entity)
    {
        await _context.UserAudits.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}