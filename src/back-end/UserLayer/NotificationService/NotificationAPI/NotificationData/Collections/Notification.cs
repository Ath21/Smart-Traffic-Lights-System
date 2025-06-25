/*
 * NotificationData.Collections.Notification
 *
 * This class represents a notification entry in the Mongo DB database of the Notification Service.
 * It contains properties for the notification ID, type, recipient ID, recipient email, message,
 * status, and timestamp.
 *
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationData.Collections;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("type")]
    public string Type { get; set; }

    [BsonElement("recipientId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid RecipientId { get; set; }

    [BsonElement("recipientEmail")]
    public string? RecipientEmail { get; set; }

    [BsonElement("message")]
    public string Message { get; set; }

    [BsonElement("status")]
    public string Status { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
