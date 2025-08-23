using AutoMapper;
using UserData.Entities;
using UserStore.Business.Password;
using UserStore.Business.Token;
using UserStore.Models;
using UserStore.Publishers;
using UserStore.Publishers.Logs;
using UserStore.Publishers.Notifications;
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
    private readonly IUserLogPublisher _userLogPublisher;
    private readonly IUserNotificationPublisher _notificationPublisher;

    public UsrService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ISessionRepository sessionRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        IUserLogPublisher userLogPublisher,
        IUserNotificationPublisher notificationPublisher)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _sessionRepository = sessionRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _userLogPublisher = userLogPublisher;
        _notificationPublisher = notificationPublisher;
    }

    public async Task<UserDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials");

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = token,
            ExpiresAt = expiresAt,
        };

        await _sessionRepository.AddAsync(session);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Logged In",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} logged in"
        });

        await _userLogPublisher.PublishAuditAsync("User Logged In", $"User {user.Username} logged in", new { user.UserId });
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} logged in");

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
        };
    }

    public async Task LogoutAsync(string token)
    {
        var session = await _sessionRepository.GetByTokenAsync(token);
        if (session == null) return;

        await _sessionRepository.DeleteByTokenAsync(token);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = session.UserId,
            Action = "User Logged Out",
            Timestamp = DateTime.UtcNow,
            Details = $"User {session.UserId} logged out"
        });

        await _userLogPublisher.PublishAuditAsync("User Logged Out", $"User {session.UserId} logged out", new { session.UserId });
        await _userLogPublisher.PublishInfoAsync($"User {session.UserId} logged out");
    }

    public async Task<UserDto> RegisterAsync(RegisterUserDto request)
    {
        if (await _userRepository.ExistsAsync(request.Username, request.Email))
            throw new InvalidOperationException("Username or email already exists");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = Enum.TryParse<UserRole>(request.Role ?? "User", true, out var parsedRole) ? parsedRole : UserRole.User,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Registered",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} registered"
        });

        await _userLogPublisher.PublishAuditAsync("User Registered", $"User {user.Username} registered", new { user.UserId });
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} registered");

        return _mapper.Map<UserDto>(user);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "Password Reset",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} reset password"
        });

        await _userLogPublisher.PublishAuditAsync("Password Reset", $"User {user.Username} reset password", new { user.UserId });
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} reset password");
    }

    public async Task SendNotificationRequestAsync(Guid userId, string message, string type)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        await _notificationPublisher.PublishNotificationAsync(user.UserId, type, message);
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        _mapper.Map(request, user);
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "Profile Updated",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} updated profile"
        });

        await _userLogPublisher.PublishAuditAsync("Profile Updated", $"User {user.Username} updated profile", new { user.UserId });
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} updated profile");
    }
}
