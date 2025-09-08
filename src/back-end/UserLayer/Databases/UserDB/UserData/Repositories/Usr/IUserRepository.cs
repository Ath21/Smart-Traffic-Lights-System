using UserData.Entities;

namespace UserData.Repositories.Usr;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByUsernameOrEmailAsync(string input);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string username, string email);
}
