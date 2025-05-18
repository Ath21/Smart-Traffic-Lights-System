/*
 * UserStore.Business.Token.ITokenService
 *
 * This interface defines a contract for generating JWT tokens for users in the UserStore application.
 * It contains a method for generating a token based on the user's information.
 * The ITokenService interface is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using UserData.Entities;

namespace UserStore.Business.Token;

public interface ITokenService
{
    (string toke, DateTime expiration) GenerateToken(User user);
}
