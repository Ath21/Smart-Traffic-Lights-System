namespace UserStore.Models;

public class UpdateProfileRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; } // Optional — if provided, update password
    public string? Status { get; set; }
    public string? Role { get; set; } // Admins can update
}
