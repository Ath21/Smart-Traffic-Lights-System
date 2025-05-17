using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserStore.Business.Usr;
using UserStore.Models;

namespace UserStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsrService _userService;

        public UserController(IUsrService userService)
        {
            _userService = userService;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _userService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _userService.LoginAsync(request);
            return Ok(result);
        }

        // POST: api/user/logout
        [HttpPost("logout")]
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

        // GET: api/user/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        // PUT: api/user/update
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto updateUserProfileDto)
        {
            var userId = GetUserId();
            await _userService.UpdateProfileAsync(userId, updateUserProfileDto);
            return NoContent();
        }

        // POST: api/user/reset-password
        [HttpPost("reset-password")]
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
