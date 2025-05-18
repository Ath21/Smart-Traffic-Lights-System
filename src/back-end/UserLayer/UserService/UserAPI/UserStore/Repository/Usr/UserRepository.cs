/*
 * UserStore.Repository.Usr.UserRepository
 *
 * This class implements the IUserRepository interface and provides methods for performing CRUD operations on User entities.
 * It uses Entity Framework Core to interact with the database.
 * The methods include:
 * - CreateAsync: Adds a new User entity to the database.
 * - GetAllAsync: Retrieves all User entities from the database.
 * - GetUserByIdAsync: Retrieves a User entity by its unique identifier (UserId).
 * - GetUserByUsernameOrEmailAsync: Retrieves a User entity by its username or email address.
 * - UpdateAsync: Updates an existing User entity in the database.
 * - UserExistsAsync: Checks if a User entity exists in the database by its username or email address.
 * The class uses the UserDbContext to interact with the database and perform the necessary operations.
 * The UserRepository class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using UserData;
using UserData.Entities;

namespace UserStore.Repository.Usr;

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
