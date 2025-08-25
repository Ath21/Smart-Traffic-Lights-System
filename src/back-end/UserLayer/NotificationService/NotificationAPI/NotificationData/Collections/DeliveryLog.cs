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

    [BsonElement("recipient")]
    public string Recipient { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    [BsonElement("sent_at")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
