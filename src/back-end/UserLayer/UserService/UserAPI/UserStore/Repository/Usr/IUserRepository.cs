/*
 * UserStore.Repository.Usr.IUserRepository
 *
 * This interface defines the contract for the User repository in the UserStore application.
 * It contains methods for performing CRUD operations on User entities.
 * The methods include:
 * - GetAllAsync: Retrieves all User entities from the database.
 * - GetUserByIdAsync: Retrieves a User entity by its unique identifier (UserId).
 * - GetUserByUsernameOrEmailAsync: Retrieves a User entity by its username or email address.
 * - CreateAsync: Creates a new User entity in the database.
 * - UpdateAsync: Updates an existing User entity in the database.
 * - UserExistsAsync: Checks if a User entity exists in the database by its username or email address.
 */
using UserData.Entities;

namespace UserStore.Repository.Usr;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User> GetUserByUsernameOrEmailAsync(string input);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> UserExistsAsync(string username, string email);
}
