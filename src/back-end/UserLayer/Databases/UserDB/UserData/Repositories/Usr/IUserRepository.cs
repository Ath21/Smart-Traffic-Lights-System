using UserData.Entities;

namespace UserData.Repositories.Usr;

public interface IUserRepository
{
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<IEnumerable<UserEntity>> GetActiveUsersAsync();
    Task InsertAsync(UserEntity user);
    Task UpdateAsync(UserEntity user);
}