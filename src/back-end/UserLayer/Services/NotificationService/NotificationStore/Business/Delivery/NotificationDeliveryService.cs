using System;
using NotificationData.Repositories.DeliveryLogs;
using NotificationStore.Models.Requests;
using NotificationStore.Models.Responses;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Business.Delivery;

public class NotificationDeliveryService : INotificationDeliveryService
{
    private readonly IDeliveryLogRepository _logRepo;
    private readonly ILogPublisher _logPublisher;

    public NotificationDeliveryService(
        IDeliveryLogRepository logRepo,
        ILogPublisher logPublisher)
    {
        _logRepo = logRepo;
        _logPublisher = logPublisher;
    }

    public async Task<IEnumerable<DeliveryResponse>> GetUserDeliveriesAsync(string userId, bool unreadOnly = false)
    {
        var logs = await _logRepo.GetUserDeliveriesAsync(userId, unreadOnly);

        await _logPublisher.PublishAuditAsync(
            "Business",
            $"[BUSINESS][DELIVERY] Fetched {(unreadOnly ? "unread" : "all")} deliveries for {userId}");

        return logs.Select(l => new DeliveryResponse
        {
            DeliveryId = l.Id,
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            MessageId = l.MessageId,
            DeliveredAt = l.DeliveredAt,
            Status = l.Status,
            Read = l.Status.Equals("Read", StringComparison.OrdinalIgnoreCase)
        });
    }

    public async Task MarkAsReadAsync(MarkAsReadRequest request)
    {
        await _logRepo.MarkAsReadAsync(request.UserId, request.UserEmail, request.DeliveryId);

        await _logPublisher.PublishAuditAsync(
            "Business",
            $"[BUSINESS][DELIVERY] Marked {request.DeliveryId} as read for {request.UserId} ({request.UserEmail})");
    }
}