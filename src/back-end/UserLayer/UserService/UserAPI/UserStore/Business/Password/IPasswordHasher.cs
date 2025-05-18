/*
 * UserStore.Business.Password.IPasswordHasher
 *
 * This namespace contains the IPasswordHasher interface, which defines methods for hashing and verifying passwords.
 * The IPasswordHasher interface is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 *
 * Methods:
 *   - bool VerifyPassword(string hash, string password): Verifies if the provided password matches the given hash.
 *   - string HashPassword(string password): Generates a hash for the provided password.
 */

namespace UserStore.Business.Password;

public interface IPasswordHasher
{
    bool VerifyPassword(string hash, string password);
    string HashPassword(string password);
}
