/*
 * UserStore.Business.Password.PasswordHasher
 *
 * This class implements the IPasswordHasher interface, providing methods for hashing and verifying passwords.
 * It uses PBKDF2 with SHA-256 for hashing and includes a salt for added security.
 * The PasswordHasher class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 *
 * Methods:
 *   - string HashPassword(string password): Generates a hash for the provided password.
 *   - bool VerifyPassword(string hash, string password): Verifies if the provided password matches the given hash.
 *
 * The hash format is: {iterations}.{salt}.{hash}
 *   - iterations: The number of iterations used in PBKDF2.
 *   - salt: The base64-encoded salt used in hashing.
 *   - hash: The base64-encoded hash generated from the password and salt.
 *
 * The PasswordHasher class uses RandomNumberGenerator to create a secure random salt.
 * It also uses Rfc2898DeriveBytes to derive the key from the password and salt.
 * The CryptographicOperations.FixedTimeEquals method is used to compare the computed hash with the stored hash
 * in a time-constant manner to prevent timing attacks.
 * The PasswordHasher class is designed to be secure and efficient, making it suitable for use in applications that require strong password hashing.
 * The PasswordHasher class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using System.Security.Cryptography;

namespace UserStore.Business.Password;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;

    // Format: {iterations}.{salt}.{hash}
    public string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
        
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string hash, string password)
    {
        var parts = hash.Split('.');
        if (parts.Length != 3)
            return false;

        int iterations = int.Parse(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] storedKey = Convert.FromBase64String(parts[2]);

        byte[] computedKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            storedKey.Length);
        
        return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
    }
}
