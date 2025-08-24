using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationData.Collections;

public class DeliveryLog
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid DeliveryId { get; set; }

    public Guid NotificationId { get; set; }
    public string Recipient { get; set; }
    public string Status { get; set; }
    public DateTime SentAt { get; set; }
}
