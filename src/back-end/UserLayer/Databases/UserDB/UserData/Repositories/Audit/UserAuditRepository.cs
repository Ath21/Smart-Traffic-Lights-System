using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserData;
using UserData.Entities;

namespace UserData.Repositories.Audit;

public class UserAuditRepository : IUserAuditRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserAuditRepository> _logger;
    private const string domain = "[REPOSITORY][USER_AUDIT]";

    public UserAuditRepository(UserDbContext context, ILogger<UserAuditRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserAuditEntity>> GetUserAuditsAsync(int userId)
    {
        _logger.LogInformation("{Domain} Retrieving audits for user {UserId}\n", domain, userId);
        return await _context.UserAudits
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task InsertAsync(UserAuditEntity entity)
    {
        _logger.LogInformation("{Domain} Inserting audit for user {UserId}\n", domain, entity.UserId);
        await _context.UserAudits.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}