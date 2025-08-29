using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserStore.Business.Usr;
using UserStore.Models.Requests;
using UserStore.Models.Responses;

namespace UserStore.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUsrService _userService;

    public UserController(IUsrService userService)
    {
        _userService = userService;
    }

    // ============================================================
    // POST: /api/users/register
    // Roles: Anonymous
    // Purpose: Register a new user
    // ============================================================
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return BadRequest("All fields are required.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var result = await _userService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetProfile), new { id = result.UserId }, result);
    }

    // ============================================================
    // POST: /api/users/login
    // Roles: Anonymous
    // Purpose: Authenticate user and issue JWT token
    // ============================================================
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        return Ok(result);
    }

    // ============================================================
    // POST: /api/users/logout
    // Roles: User, TrafficOperator, Admin
    // Purpose: Invalidate JWT token
    // ============================================================
    [HttpPost("logout")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        await _userService.LogoutAsync(token);
        return NoContent();
    }

    // ============================================================
    // GET: /api/users/profile
    // Roles: User, TrafficOperator, Admin
    // Purpose: Get current user profile
    // ============================================================
    [HttpGet("profile")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    // ============================================================
    // PUT: /api/users/update
    // Roles: User, TrafficOperator, Admin
    // Purpose: Update user profile (username, email, password)
    // ============================================================
    [HttpPut("update")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Username and Email are required.");
        }

        if (!string.IsNullOrEmpty(request.Password) &&
            request.Password != request.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var userId = GetUserId();
        await _userService.UpdateProfileAsync(userId, request);
        return NoContent();
    }

    // ============================================================
    // POST: /api/users/reset-password
    // Roles: Anonymous
    // Purpose: Reset password via email or token
    // ============================================================
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _userService.ResetPasswordAsync(request);
        return NoContent();
    }

    // ============================================================
    // POST: /api/users/send-notification-request
    // Roles: User, TrafficOperator, Admin
    // Purpose: Request sending a notification (delegated to Notification Service)
    // ============================================================
    [HttpPost("send-notification-request")]
    [Authorize(Roles = "User,TrafficOperator,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message) || string.IsNullOrWhiteSpace(request.Type))
            return BadRequest("Message and Type are required.");

        var userId = GetUserId();
        await _userService.SendNotificationRequestAsync(userId, request.Message, request.Type);

        return Ok(new { status = "sent", message = request.Message, type = request.Type });
    }

    // Helper to extract UserId from JWT Claims
    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(ClaimTypes.Name);

        return Guid.TryParse(userId, out var guid)
            ? guid
            : throw new UnauthorizedAccessException("Invalid token.");
    }
}
