using AutoMapper;
using UserData.Entities;
using UserStore.Business.Password;
using UserStore.Business.Token;
using UserStore.Models.Dtos;
using UserStore.Models.Requests;
using UserStore.Models.Responses;
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

    // GET profile
    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        return _mapper.Map<UserProfileResponse>(user);
    }

    // LOGIN
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password");

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = token,
            ExpiresAt = expiresAt
        };
        await _sessionRepository.AddAsync(session);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "LOGIN",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} logged in"
        });

        await _userLogPublisher.PublishAuditAsync("LOGIN", $"User {user.Username} logged in", new { user.UserId });

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserDto>(user)
        };
    }

    // LOGOUT
    public async Task LogoutAsync(string token)
    {
        var session = await _sessionRepository.GetByTokenAsync(token);
        if (session == null) return;

        await _sessionRepository.DeleteByTokenAsync(token);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = session.UserId,
            Action = "LOGOUT",
            Timestamp = DateTime.UtcNow,
            Details = $"User {session.UserId} logged out"
        });

        await _userLogPublisher.PublishAuditAsync("LOGOUT", $"User {session.UserId} logged out", new { session.UserId });
    }

    // REGISTER
    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
    {
        if (await _userRepository.ExistsAsync(request.Username, request.Email))
            throw new InvalidOperationException("Username or email already exists");

        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");

        var user = _mapper.Map<User>(request);
        user.UserId = Guid.NewGuid();
        user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        user.Role = UserRole.User; // ή Admin/Operator αν θες conditional
        user.Status = "Active";
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.AddAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "REGISTER",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} registered"
        });

        await _userLogPublisher.PublishAuditAsync("REGISTER", $"User {user.Username} registered", new { user.UserId });

        return _mapper.Map<UserResponse>(user);
    }

    // RESET PASSWORD
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.Email);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "RESET_PASSWORD",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} reset password"
        });

        await _userLogPublisher.PublishAuditAsync("RESET_PASSWORD", $"User {user.Username} reset password", new { user.UserId });
    }

    // SEND NOTIFICATION REQUEST
    public async Task SendNotificationRequestAsync(Guid userId, string message, string type)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        await _notificationPublisher.PublishNotificationAsync(
            user.UserId,
            user.Email,
            type,
            message,
            "User"
        );
    }

    // UPDATE PROFILE
    public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.Username = request.Username;
        user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.Password))
        {
            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "UPDATE_PROFILE",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} updated profile"
        });

        await _userLogPublisher.PublishAuditAsync("UPDATE_PROFILE", $"User {user.Username} updated profile", new { user.UserId });

        return _mapper.Map<UserResponse>(user);
    }
}
