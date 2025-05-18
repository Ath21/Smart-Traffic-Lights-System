/*
 * UserStore.Controllers.UserController
 *
 * This class represents the controller for user-related operations in the UserStore application.
 * It contains methods for user registration, login, logout, profile retrieval, profile update,
 * and password reset.
 * The UserController class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 *
 * API Endpoints:
 *   POST   API/User/Register       - Register a new user
 *   POST   API/User/Login          - Login a user
 *   POST   API/User/Logout         - Logout the current user (requires authorization)
 *   GET    API/User/Profile        - Get the current user's profile (requires authorization)
 *   PUT    API/User/Update         - Update the current user's profile (requires authorization)
 *   POST   API/User/Reset-Password - Reset a user's password
 */
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserStore.Business.Usr;
using UserStore.Models;

namespace UserStore.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsrService _userService;

        public UserController(IUsrService userService)
        {
            _userService = userService;
        }

        // POST: API/User/Register
        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _userService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }

        // POST: API/User/Login
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _userService.LoginAsync(request);
            return Ok(result);
        }

        // POST: API/User/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            await _userService.LogoutAsync(token);
            return NoContent();
        }

        // GET: API/User/Profile
        [HttpGet("Profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        // PUT: API/User/Update
        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto updateUserProfileDto)
        {
            var userId = GetUserId();
            await _userService.UpdateProfileAsync(userId, updateUserProfileDto);
            return NoContent();
        }

        // POST: API/User/Reset-Password
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequestDto)
        {
            await _userService.ResetPasswordAsync(resetPasswordRequestDto);
            return NoContent();
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name);
            return Guid.TryParse(userId, out var guid) ? guid : throw new UnauthorizedAccessException("Invalid token.");
        }
    }
}
