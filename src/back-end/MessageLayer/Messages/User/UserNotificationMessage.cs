namespace Messages.User;

public class UserNotificationMessage : BaseMessage
{
  public string? NotificationType { get; set; }
  public string? Title { get; set; }
  public string? Body { get; set; } 
  public string? RecipientEmail { get; set; } 
  public string? Status { get; set; } 
}
