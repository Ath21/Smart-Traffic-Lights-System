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

    // POST: api/users/register
    [HttpPost("register")]
    [AllowAnonymous]
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

    // POST: api/users/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        return Ok(result);
    }

    // POST: api/users/logout
    [HttpPost("logout")]
    [Authorize] // any logged-in role
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        await _userService.LogoutAsync(token);
        return NoContent();
    }

    // GET: api/users/me
    [HttpGet("profile")]
    [Authorize(Roles = "User,Admin,TrafficOperator")] // Viewer δεν βλέπει προφίλ
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    // PUT: api/users/update
    [HttpPut("update")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
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

    // POST: api/users/reset-password
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _userService.ResetPasswordAsync(request);
        return NoContent();
    }

    // POST: api/users/send-notification-request
    [HttpPost("send-notification-request")]
    [Authorize(Roles = "User,Admin,TrafficOperator")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        var userId = GetUserId();
        await _userService.SendNotificationRequestAsync(userId, request.Message, request.Type);

        return Ok(new { status = "sent", message = request.Message, type = request.Type });
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(ClaimTypes.Name);

        return Guid.TryParse(userId, out var guid)
            ? guid
            : throw new UnauthorizedAccessException("Invalid token.");
    }
}
