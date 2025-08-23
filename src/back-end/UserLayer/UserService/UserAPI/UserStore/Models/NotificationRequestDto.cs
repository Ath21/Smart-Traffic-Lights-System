namespace UserStore.Models;

public class NotificationRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g. "VerifyEmail", "PasswordReset"
}
