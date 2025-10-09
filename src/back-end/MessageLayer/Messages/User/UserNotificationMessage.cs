// user.notification.{type}
//
// {type} : alert, public-notice, private, request
//
// Published by : Notification Service, User Service
// Consumed by  : User Service, Notification Service
namespace Messages.User;

public class UserNotificationMessage : BaseMessage
{
  public string? NotificationType { get; set; } // Alert, PublicNotice, Private, Request
  public string? Title { get; set; } // message title
  public string? Body { get; set; } // message body content
  public string? RecipientEmail { get; set; } // target user email
  public string? Status { get; set; } // Pending, Sent, Failed
}

/*

user.notification.alert

{
  "CorrelationId": "e1a4b657-4422-4c8d-86f5-f01b89c1f333",
  "Timestamp": "2025-10-08T09:30:00Z",
  "SourceServices": ["Notification Service"],
  "DestinationServices": ["User Service"],
  "NotificationType": "Alert",
  "Title": "Traffic Alert - Agiou Spyridonos",
  "Body": "Adaptive control activated due to congestion index 0.82.",
  "RecipientEmail": "all@uniwa-stls",
  "Status": "Broadcasted",
  "IntersectionId": 2,
  "IntersectionName": "Agiou Spyridonos"
}

user.notification.private

{
  "CorrelationId": "bb3f6cc5-9ee1-4a6c-91aa-1f80a423b78f",
  "Timestamp": "2025-10-08T10:00:00Z",
  "SourceServices": ["User Service"],
  "DestinationServices": ["Notification Service"],
  "NotificationType": "Private",
  "Title": "Manual Override Applied",
  "Body": "Intersection 'Kentriki Pyli' set to Manual mode by Operator 'vmamalis'.",
  "RecipientEmail": "vathanas1ou@uniwa.gr",
  "Status": "Sent",
  "IntersectionId": 5,
  "IntersectionName": "Kentriki Pyli"
}

user.notification.public-notice

{
  "CorrelationId": "d4c3f8e2-5b6a-4f7d-9c3e-2a1b0c4d5e6f",
  "Timestamp": "2025-10-08T09:45:00Z",
  "SourceServices": ["Notification Service"],
  "DestinationServices": ["User Service"],
  "NotificationType": "PublicNotice",
  "Title": "Scheduled Maintenance",
  "Body": "System maintenance will occur tonight between 00:00–02:00. Traffic lights will remain in safe mode.",
  "RecipientEmail": "all@uniwa-stls",
  "Status": "Broadcasted"
}

user.notification.request

{
  "CorrelationId": "e8aa1c1f-731f-49b4-986c-bacb2b382af3",
  "Timestamp": "2025-10-08T10:05:00Z",
  "SourceServices": ["User Service"],
  "DestinationServices": ["Notification Service"],
  "NotificationType": "Request",
  "Title": "Traffic Light Fault Report",
  "Body": "Pedestrian button at 'Anatoliki Pyli' appears non-responsive.",
  "RecipientEmail": "vmamalis@uniwa.gr",
  "Status": "Pending",
  "IntersectionId": 4,
  "IntersectionName": "Anatoliki Pyli"
}

*/