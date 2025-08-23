namespace UserStore.Models;

public class ResetPasswordRequestDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
