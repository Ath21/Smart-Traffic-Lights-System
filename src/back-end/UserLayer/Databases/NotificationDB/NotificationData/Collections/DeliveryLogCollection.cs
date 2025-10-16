using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationData.Collections;

// Updated by : Notification Service
// Read by    : Notification Service
[BsonDiscriminator("delivery_logs")]
[BsonIgnoreExtraElements]
public class DeliveryLogCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string DeliveryId { get; set; } = string.Empty;

    [BsonElement("notification_id")]
    public string NotificationId { get; set; } = string.Empty;

    [BsonElement("recipient_email")]
    public string RecipientEmail { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Broadcasted";

    [BsonElement("delivered_at")]
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    [BsonElement("delivery_method")]
    public string DeliveryMethod { get; set; } = "Email";

    [BsonElement("is_read")]
    public bool IsRead { get; set; } = false;

    [BsonElement("read_at")]
    [BsonIgnoreIfNull]
    public DateTime? ReadAt { get; set; } = null;
}

/*
Example new doc:
{
  "DeliveryId": "67119a59e4b0b0bdf8700a2d",
  "NotificationId": "6711984ee4b0b0bdf8700a28",
  "RecipientEmail": "vathanas1ou@uniwa.gr",
  "Status": "Delivered",
  "DeliveredAt": "2025-10-08T10:06:00Z",
  "DeliveryMethod": "Email",
  "IsRead": true,
  "ReadAt": "2025-10-08T12:02:00Z"
}
*/
