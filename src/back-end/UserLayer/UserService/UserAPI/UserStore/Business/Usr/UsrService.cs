/*
 * UserStore.Business.Usr.UsrService
 *
 * This class implements the IUsrService interface and provides methods for user-related operations
 * such as registration, login, profile management, password reset, and logout.
 * The UsrService class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 *
 * Dependencies:
 *   - IUserRepository: Interface for user data access operations.
 *   - IAuditLogRepository: Interface for audit log data access operations.
 *   - ISessionRepository: Interface for session data access operations.
 *   - ITokenService: Interface for generating JWT tokens.
 *   - IPasswordHasher: Interface for hashing and verifying passwords.
 *   - IMapper: Interface for object mapping (e.g., AutoMapper).
 *   - IPublishEndpoint: Interface for publishing messages to a message broker (e.g., MassTransit).
 *
 * Methods:
 *   - Task<UserProfileDto> GetProfileAsync(Guid userId): Retrieves the user's profile information.
 *   - Task<LoginResponseDto> LoginAsync(LoginRequestDto request): Authenticates the user and generates a JWT token.
 *   - Task LogoutAsync(string token): Logs out the user by invalidating the session.
 *   - Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request): Registers a new user.
 *   - Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto): Resets the user's password.
 *   - Task UpdateProfileAsync(Guid userId, UpdateProfileRequestDto updateUserProfileDto): Updates the user's profile information.
 */
using AutoMapper;
using MassTransit;
using UserData.Entities;
using UserStore.Business.Password;
using UserStore.Business.Token;
using UserStore.Models;
using UserStore.Publishers;
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

    public UsrService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ISessionRepository sessionRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        IUserLogPublisher userLogPublisher)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _sessionRepository = sessionRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _userLogPublisher = userLogPublisher;
    }

    // GET: /API/User/Profile
    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return _mapper.Map<UserProfileDto>(user);
    }

    // POST: /API/User/Login
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Validate the request
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.Username);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Generate JWT token and create session
        var (token, expiresAt) = _tokenService.GenerateToken(user);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = token,
            ExpiresAt = expiresAt,
        };

        await _sessionRepository.CreateSessionAsync(session);

        // Log the login action
        await _auditLogRepository.LogActionSync(new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = user.UserId,
            Action = "User Logged In",
            Timestamp = DateTime.UtcNow,
            Details = $"User {user.Username} logged in successfully."
        });

        //await _userLogPublisher.PublishAuditAsync(user.UserId, "User Logged In", $"User {user.Username} logged in successfully.");
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} logged in successfully.");

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
        };
    }

    // POST: /API/User/Logout
    public async Task LogoutAsync(string token)
    {
        var session = await _sessionRepository.GetSessionByTokenAsync(token);
        if (session != null)
        {
            await _sessionRepository.DeleteSessionAsync(token);

            await _auditLogRepository.LogActionSync(new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = session.UserId,
                Action = "User Logged Out",
                Timestamp = DateTime.UtcNow,
                Details = $"User {session.User.Username} logged out successfully."
            });

            //await _userLogPublisher.PublishAuditAsync(session.UserId, "User Logged Out", $"User {session.User.Username} logged out successfully.");
            await _userLogPublisher.PublishInfoAsync($"User {session.User.Username} logged out successfully.");
        }
    }

    // POST: /API/User/Register
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

        //await _userLogPublisher.PublishAuditAsync(user.UserId, "User Registered", $"User {user.Username} registered successfully.");
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} registered successfully.");

        return new RegisterResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status
        };
    }

    // POST: /API/User/ResetPassword
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

        //await _userLogPublisher.PublishAuditAsync(user.UserId, "User Password Reset", $"User {user.Username} reset their password successfully.");
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} reset their password successfully.");
    }

    // PUT: /API/User/UpdateProfile
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

        //await _userLogPublisher.PublishAuditAsync(user.UserId, "User Profile Updated", $"User {user.Username} updated their profile successfully.");
        await _userLogPublisher.PublishInfoAsync($"User {user.Username} updated their profile successfully.");
    }
}
