using System;

namespace UserMessages;

// user.notification.{type}
public class UserNotificationMessage
{
    public string Type { get; set; } // alert, info, request
    public string Title { get; set; }
    public string Message { get; set; }
    public string TargetAudience { get; set; } // users, operators, admins, public
    public DateTime CreatedAt { get; set; }
}
