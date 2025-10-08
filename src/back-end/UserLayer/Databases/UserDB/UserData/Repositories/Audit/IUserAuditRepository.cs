using UserData.Entities;

namespace UserData.Repositories.Audit;

public interface IUserAuditRepository
{
    Task<IEnumerable<UserAuditEntity>> GetUserAuditsAsync(int userId);
    Task InsertAsync(UserAuditEntity audit);
}