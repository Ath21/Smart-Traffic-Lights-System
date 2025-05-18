/*
 * UserStore.Models.LoginResponseDto
 *
 * This class represents the data transfer object (DTO) for login responses in the UserStore application.
 * It contains properties for the authentication token and the expiration date of the token.
 * The LoginResponseDto class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
namespace UserStore.Models;

public class LoginResponseDto
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
