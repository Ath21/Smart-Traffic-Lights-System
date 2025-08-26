using UserStore.Models;
using UserStore.Models.Requests;
using UserStore.Models.Responses;

namespace UserStore.Business.Usr;

public interface IUsrService
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string token);
    Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task SendNotificationRequestAsync(Guid userId, string message, string type);
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}
