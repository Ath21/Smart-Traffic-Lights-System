using AutoMapper;
using UserData.Entities;
using UserStore.Business.Password;
using UserStore.Business.Token;
using UserStore.Models.Requests;
using UserStore.Models.Responses;
using UserStore.Publishers.Logs;
using UserStore.Publishers.Notifications;
using UserData.Repositories.Audit;
using UserData.Repositories.Ses;
using UserData.Repositories.Usr;

namespace UserStore.Business.Usr;

public class UsrService : IUsrService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAuditRepository _auditRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly IUserLogPublisher _logPublisher;
    private readonly IUserNotificationPublisher _notificationPublisher;

    private const string ServiceTag = "[BUSINESS][USER]";

    public UsrService(
        IUserRepository userRepository,
        IUserAuditRepository auditRepository,
        ISessionRepository sessionRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        IUserLogPublisher logPublisher,
        IUserNotificationPublisher notificationPublisher)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _sessionRepository = sessionRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logPublisher = logPublisher;
        _notificationPublisher = notificationPublisher;
    }

    // [POST] /api/users/register
    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
    {
        if (await _userRepository.ExistsAsync(request.Username, request.Email))
            throw new InvalidOperationException("Username or email already exists.");

        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match.");

        var user = _mapper.Map<UserEntity>(request);
        user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        user.Role = "User";
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        await _userRepository.InsertAsync(user);

        await _auditRepository.InsertAsync(new UserAuditEntity
        {
            UserId = user.UserId,
            Action = "REGISTER",
            Details = $"{ServiceTag} User '{user.Username}' registered.",
            Timestamp = DateTime.UtcNow
        });

        await _logPublisher.PublishAuditAsync(
            "REGISTER",
            $"{ServiceTag} User '{user.Username}' registered.",
            new Dictionary<string, string> { { "UserId", user.UserId.ToString() } });

        return _mapper.Map<UserResponse>(user);
    }

    // [POST] /api/users/login
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
                   ?? await _userRepository.GetByUsernameAsync(request.Email);

        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash!, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        var session = new SessionEntity
        {
            UserId = user.UserId,
            Session = token,
            LoginTime = DateTime.UtcNow,
            IsActive = true
        };
        await _sessionRepository.InsertAsync(session);

        await _auditRepository.InsertAsync(new UserAuditEntity
        {
            UserId = user.UserId,
            Action = "LOGIN",
            Details = $"{ServiceTag} User '{user.Username}' logged in.",
            Timestamp = DateTime.UtcNow
        });

        await _logPublisher.PublishAuditAsync(
            "LOGIN",
            $"{ServiceTag} User '{user.Username}' logged in.",
            new Dictionary<string, string> { { "UserId", user.UserId.ToString() } });

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    // [POST] /api/users/logout
    public async Task LogoutAsync(string token)
    {
        var session = await _sessionRepository.GetByTokenAsync(token);
        if (session == null) return;

        await _sessionRepository.DeleteAsync(session);

        await _auditRepository.InsertAsync(new UserAuditEntity
        {
            UserId = session.UserId,
            Action = "LOGOUT",
            Details = $"{ServiceTag} User {session.UserId} logged out.",
            Timestamp = DateTime.UtcNow
        });

        await _logPublisher.PublishAuditAsync(
            "LOGOUT",
            $"{ServiceTag} User {session.UserId} logged out.",
            new Dictionary<string, string> { { "UserId", session.UserId.ToString() } });
    }

    // [GET] /api/users/profile
    public async Task<UserProfileResponse> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var profile = _mapper.Map<UserProfileResponse>(user);
        profile.Status = user.IsActive ? "Active" : "Inactive";
        profile.UpdatedAt = DateTime.UtcNow;
        return profile;
    }

    // [PUT] /api/users/update
    public async Task<UserResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Username = request.Username;
        user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.Password))
        {
            if (request.Password != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(user);

        await _auditRepository.InsertAsync(new UserAuditEntity
        {
            UserId = user.UserId,
            Action = "UPDATE_PROFILE",
            Details = $"{ServiceTag} User '{user.Username}' updated profile.",
            Timestamp = DateTime.UtcNow
        });

        await _logPublisher.PublishAuditAsync(
            "UPDATE_PROFILE",
            $"{ServiceTag} User '{user.Username}' updated profile.",
            new Dictionary<string, string> { { "UserId", user.UserId.ToString() } });

        return _mapper.Map<UserResponse>(user);
    }

    // [POST] /api/users/reset-password
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
                   ?? await _userRepository.GetByUsernameAsync(request.Email);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        await _auditRepository.InsertAsync(new UserAuditEntity
        {
            UserId = user.UserId,
            Action = "RESET_PASSWORD",
            Details = $"{ServiceTag} User '{user.Username}' reset password.",
            Timestamp = DateTime.UtcNow
        });

        await _logPublisher.PublishAuditAsync(
            "RESET_PASSWORD",
            $"{ServiceTag} User '{user.Username}' reset password.",
            new Dictionary<string, string> { { "UserId", user.UserId.ToString() } });
    }

    // [POST] /api/users/send-notification-request
    public async Task SendNotificationRequestAsync(int userId, string message, string type)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        await _notificationPublisher.PublishNotificationRequestAsync(
            user.Username,
            "Pending",
            null,
            new Dictionary<string, string>
            {
                { "Message", message },
                { "Type", type }
            }
        );

        await _auditRepository.InsertAsync(new UserAuditEntity
        {
            UserId = user.UserId,
            Action = "SEND_NOTIFICATION",
            Details = $"{ServiceTag} Sent notification ({type}) to '{user.Email}'.",
            Timestamp = DateTime.UtcNow
        });

        await _logPublisher.PublishAuditAsync(
            "SEND_NOTIFICATION",
            $"{ServiceTag} Sent notification ({type}) to '{user.Email}'.",
            new Dictionary<string, string>
            {
                { "UserId", user.UserId.ToString() },
                { "Type", type }
            });
    }
}
