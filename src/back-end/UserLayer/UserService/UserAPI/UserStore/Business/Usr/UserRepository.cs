using System;
using UserData;
using UserData.Entities;

namespace UserStore.Business.Usr;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }
    
    public Task CreateAsync(User user)
    {
        _context.Users.Add(user);
        return _context.SaveChangesAsync();
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<User>>(_context.Users.ToList());
    }

    public Task<User> GetUserByIdAsync(Guid userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        return Task.FromResult(user);
    }

    public Task<User> GetUserByUsernameOrEmailAsync(string input)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == input || u.Email == input);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
        if (existingUser != null)
        {
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.Status = user.Status;
            existingUser.UpdatedAt = DateTime.UtcNow;

            return _context.SaveChangesAsync();
        }

        return Task.CompletedTask;
    }

    public Task<bool> UserExistsAsync(string username, string email)
    {
        var exists = _context.Users.Any(u => u.Username == username || u.Email == email);
        return Task.FromResult(exists);
    }
}
