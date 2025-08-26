using UserData.Entities;

namespace UserStore.Models.Dtos;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string Status { get; set; } = "Active";
}
