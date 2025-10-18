namespace Messages.User;

public class UserNotificationMessage
{
    public string UserId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string Type { get; set; } = "public";  // public | private | alert
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
