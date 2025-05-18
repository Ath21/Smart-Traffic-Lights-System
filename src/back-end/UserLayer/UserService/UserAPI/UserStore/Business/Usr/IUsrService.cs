/*
 * UserStore.Business.Usr.IUsrService
 *
 * This interface defines the contract for user-related services in the UserStore application.
 * It includes methods for user registration, login, profile management, password reset, and logout.
 * The IUsrService interface is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using UserStore.Models;

namespace UserStore.Business.Usr;

public interface IUsrService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequestDto updateUserProfileDto);
    Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto);
    Task LogoutAsync(string token);
}
