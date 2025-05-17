using System;
using UserData.Entities;

namespace UserStore.Business.Usr;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User> GetUserByUsernameOrEmailAsync(string input);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> UserExistsAsync(string username, string email);
}
