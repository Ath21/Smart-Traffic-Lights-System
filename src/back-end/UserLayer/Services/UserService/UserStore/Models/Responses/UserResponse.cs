using UserData.Entities;

namespace UserStore.Models.Responses;

public class UserResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string Status { get; set; } = "Active";
}
