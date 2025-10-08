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

/*

{
  "DeliveryId": "67119a42e4b0b0bdf8700a2b",
  "NotificationId": "671197b7e4b0b0bdf8700a21",
  "Status": "Broadcasted",
  "DeliveredAt": "2025-10-08T09:31:00Z",
  "DeliveryMethod": "Email"
},
{
  "DeliveryId": "67119a59e4b0b0bdf8700a2d",
  "NotificationId": "6711984ee4b0b0bdf8700a28",
  "Status": "Delivered",
  "DeliveredAt": "2025-10-08T10:06:00Z",
  "DeliveryMethod": "Email"
}
*/