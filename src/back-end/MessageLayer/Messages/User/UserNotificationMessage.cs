// user.notification.{type}
//
// {type} : alert, public-notice, private, request
//
// Published by : Notification Service
// Consumed by  : User Service
public class UserNotificationMessage : BaseMessage
{
    public NotificationType NotificationType { get; set; } // Alert, PublicNotice, Private, Request
    public string Title { get; set; } = string.Empty; // message title
    public string Body { get; set; } = string.Empty;  // message body content
    public string RecipientEmail { get; set; } = string.Empty; // target user email
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending; // Pending, Sent, Failed
}