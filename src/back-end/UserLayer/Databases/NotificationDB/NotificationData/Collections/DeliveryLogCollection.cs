using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationData.Collections;

// Updated by : Notification Service
// Read by    : Notification Service
[BsonDiscriminator("delivery_logs")]
[BsonIgnoreExtraElements]
public class DeliveryLogCollection
{
    // unique delivery log ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string DeliveryId { get; set; } = string.Empty;

     // reference to parent notification
    [BsonElement("notification_id")]
    public string NotificationId { get; set; } = string.Empty;

    // delivery status (Broadcasted, Delivered, Failed)
    [BsonElement("status")]
    public string Status { get; set; } = "Broadcasted";

    // delivery timestamp (UTC)
    [BsonElement("delivered_at")]
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    // delivery channel (Email, SMS, Push)
    [BsonElement("delivery_method")]
    public string DeliveryMethod { get; set; } = "Email";
}
