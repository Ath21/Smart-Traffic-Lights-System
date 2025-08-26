using System.ComponentModel.DataAnnotations;

namespace UserStore.Models
{
    public class UpdateProfileRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Optional password change
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        public string? Status { get; set; }
        public string? Role { get; set; }
    }
}
