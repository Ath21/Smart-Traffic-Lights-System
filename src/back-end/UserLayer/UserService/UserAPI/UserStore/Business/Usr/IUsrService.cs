using UserStore.Models;

namespace UserStore.Business.Usr;

public interface IUsrService
{
    public Task<UserProfileDto> GetProfileAsync(Guid userId);
    public Task<LoginResponseDto> LoginAsync(LoginDto request);
    public Task LogoutAsync(string token);
    public Task<UserDto> RegisterAsync(RegisterUserDto request);
    public Task ResetPasswordAsync(ResetPasswordRequestDto request);
    public Task SendNotificationRequestAsync(Guid userId, string message, string type);
    public Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
}
