using UserStore.Models.Requests;
using UserStore.Models.Responses;

namespace UserStore.Business.Usr;

public interface IUsrService
{
    // [POST]   /api/users/register
    Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    // [POST]   /api/users/login
    Task<LoginResponse> LoginAsync(LoginRequest request);
    // [POST]   /api/users/logout
    Task LogoutAsync(string token);
    // [GET]    /api/users/profile
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    // [PUT]    /api/users/update
    Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    // [POST]   /api/users/reset-password
    Task ResetPasswordAsync(ResetPasswordRequest request);
    // [POST]   /api/users/send-notification-request
    Task SendNotificationRequestAsync(Guid userId, string message, string type);
}
