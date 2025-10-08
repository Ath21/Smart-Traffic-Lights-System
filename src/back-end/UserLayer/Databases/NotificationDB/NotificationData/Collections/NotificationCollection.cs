using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationData.Collections;

// Updated by : Notification Service
// Read by    : Notification Service
[BsonDiscriminator("notifications")]
[BsonIgnoreExtraElements]
public class NotificationCollection
{
    // unique notification ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string NotificationId { get; set; } = string.Empty;

    // message type (alert, broadcast, private)
    [BsonElement("type")]
    public string Type { get; set; } = "alert";

    // notification title (e.g. "Traffic Alert")
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    // body of the message
    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    // target user email
    [BsonElement("recipient_email")]
    public string RecipientEmail { get; set; } = string.Empty;

    // message state (Pending, Sent, Failed)
    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    // timestamp of message creation (UTC)
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/*

{
  "NotificationId": "671197b7e4b0b0bdf8700a21",
  "Type": "alert",
  "Title": "Traffic Alert - High Congestion at Agiou Spyridonos",
  "Message": "Traffic density has exceeded safe thresholds. Adaptive signal control activated.",
  "RecipientEmail": "all@uniwa-stls",
  "Status": "Broadcasted",
  "CreatedAt": "2025-10-08T09:30:00Z"
}


{
  "NotificationId": "671197cfe4b0b0bdf8700a23",
  "Type": "public-notice",
  "Title": "Scheduled Maintenance",
  "Message": "System maintenance will occur tonight between 00:00–02:00. Traffic lights will remain in safe mode.",
  "RecipientEmail": "all@uniwa-stls",
  "Status": "Broadcasted",
  "CreatedAt": "2025-10-08T09:45:00Z"
}


{
  "NotificationId": "67119820e4b0b0bdf8700a26",
  "Type": "private",
  "Title": "Manual Override Applied",
  "Message": "Intersection 'Kentriki Pyli' was manually overridden due to sensor calibration issue.",
  "RecipientEmail": "vathanas1ou@uniwa.gr",
  "Status": "Sent",
  "CreatedAt": "2025-10-08T10:00:00Z"
}


{
  "NotificationId": "6711984ee4b0b0bdf8700a28",
  "Type": "request",
  "Title": "Traffic Light Fault Report",
  "Message": "The pedestrian signal at 'Anatoliki Pyli' appears unresponsive.",
  "RecipientEmail": "vmamalis@uniwa.gr",
  "Status": "Pending",
  "CreatedAt": "2025-10-08T10:05:00Z"
}

*/