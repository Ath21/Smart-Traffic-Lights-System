using System;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UserData.Entities;
using UserStore.Business.PasswordHasher;
using UserStore.Business.Token;
using UserStore.Models;
using UserStore.Repository.Audit;
using UserStore.Repository.Ses;
using UserStore.Repository.Usr;

namespace UserStore.Business.Usr;

public class UsrService : IUsrService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UsrService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ISessionRepository sessionRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _sessionRepository = sessionRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.Username);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = token,
            ExpiresAt = expiresAt,
        };

        await _sessionRepository.CreateSessionAsync(session);

        await _auditLogRepository.LogActionSync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Logged In",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} logged in successfully."
        });

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
        };
    }

    public async Task LogoutAsync(string token)
    {
        var session = await _sessionRepository.GetSessionByTokenAsync(token);
        if (session == null)
        {
            await _sessionRepository.DeleteSessionAsync(token);

            await _auditLogRepository.LogActionSync(new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = session.UserId,
                Action = "User Logged Out",
                Timestamp = DateTime.UtcNow,
                Details = $"User {session.UserId} logged out successfully."
            });
        }
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.UserExistsAsync(request.Username, request.Email))
        {
            throw new InvalidOperationException("User or email already exists");
        }

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _passwordHasher.HashPassword(request.Password);

        await _userRepository.CreateAsync(user);

        await _auditLogRepository.LogActionSync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Registered",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} registered successfully."
        });

        return new RegisterResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status
        };
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(resetPasswordRequestDto.UsernameOrEmail);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.PasswordHash = _passwordHasher.HashPassword(resetPasswordRequestDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.LogActionSync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Password Reset",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} reset their password successfully."
        });
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequestDto updateUserProfileDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        _mapper.Map(updateUserProfileDto, user);
        user.PasswordHash = _passwordHasher.HashPassword(updateUserProfileDto.Password);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.LogActionSync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Profile Updated",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} updated their profile successfully."
        });
    }
}
