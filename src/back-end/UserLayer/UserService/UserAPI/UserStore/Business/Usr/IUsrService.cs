using UserStore.Models;

namespace UserStore.Business.Usr;

public interface IUsrService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequestDto updateUserProfileDto);
    Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto);
    Task LogoutAsync(string token);
    Task SendNotificationRequestAsync(Guid userId, string message, string type);
}
