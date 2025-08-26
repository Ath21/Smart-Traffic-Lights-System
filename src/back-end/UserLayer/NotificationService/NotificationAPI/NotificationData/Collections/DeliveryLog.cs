using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationData.Collections;

public class DeliveryLog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid DeliveryId { get; set; }

    [BsonElement("notification_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid NotificationId { get; set; }

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }   // ✅ new: track user by Guid

    [BsonElement("recipient_email")]
    public string RecipientEmail { get; set; } = string.Empty;  // ✅ new: explicit email

    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    [BsonElement("sent_at")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [BsonElement("is_read")]
    public bool IsRead { get; set; } = false;

}
