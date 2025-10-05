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