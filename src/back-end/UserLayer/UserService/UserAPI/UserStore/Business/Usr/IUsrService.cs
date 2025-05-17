using System;
using Microsoft.AspNetCore.Identity.Data;
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
