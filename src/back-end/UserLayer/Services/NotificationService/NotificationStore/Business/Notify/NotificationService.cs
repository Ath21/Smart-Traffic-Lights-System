using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NotificationData.Collections;
using NotificationData.Repositories.DeliveryLogs;
using NotificationData.Repositories.Notifications;
using NotificationStore.Business.Email;
using NotificationStore.Models.Dtos;
using NotificationStore.Publishers.Notifications;

namespace NotificationStore.Business.Notify;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDeliveryLogRepository _deliveryLogRepository;
    private readonly IEmailService _emailService;
    private readonly IUserNotificationPublisher _publisher;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationService> _logger;

    private const string ServiceTag = "[BUSINESS][NOTIFY]";

    public NotificationService(
        INotificationRepository notificationRepository,
        IDeliveryLogRepository deliveryLogRepository,
        IMapper mapper,
        IEmailService emailService,
        IUserNotificationPublisher publisher,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _deliveryLogRepository = deliveryLogRepository;
        _mapper = mapper;
        _emailService = emailService;
        _publisher = publisher;
        _logger = logger;
    }

    // [POST] /api/notifications/send
    // Creates a user-targeted notification, persists it, publishes event, logs delivery, emails recipient.
    public async Task SendUserNotificationAsync(int userId, string email, string message, string type)
    {
        // 1) Create notification document (Mongo ObjectId as string)
        var notifId = ObjectId.GenerateNewId().ToString();

        var notificationDoc = new NotificationCollection
        {
            NotificationId = notifId,
            Type = string.IsNullOrWhiteSpace(type) ? "private" : type.ToLowerInvariant(),
            Title = $"{type} Notification",
            Message = message,
            RecipientEmail = email,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("{Tag} Create notification {Id} for {Email}", ServiceTag, notifId, email);
        await _notificationRepository.InsertAsync(notificationDoc);

        // 2) Publish to bus (so other services can react)
        _logger.LogInformation("{Tag} Publish user alert: userId={UserId}, email={Email}", ServiceTag, userId, email);
        await _publisher.PublishUserAlertAsync(userId, email, type, message);

        // 3) Optional e-mail send (side effect)
        if (!string.IsNullOrWhiteSpace(email))
        {
            _logger.LogInformation("{Tag} Email send → {Email}", ServiceTag, email);
            await _emailService.SendEmailAsync(email, $"[Notification] {type}", message);
        }

        // 4) Mark as Sent
        await _notificationRepository.UpdateStatusAsync(notifId, "Sent");

        // 5) Delivery log entry
        var delivery = new DeliveryLogCollection
        {
            DeliveryId = ObjectId.GenerateNewId().ToString(),
            NotificationId = notifId,
            Status = "Delivered",
            DeliveredAt = DateTime.UtcNow,
            DeliveryMethod = "Email"
        };
        await _deliveryLogRepository.InsertAsync(delivery);
    }

    // [POST] /api/notifications/public-notice
    // Broadcast-style notice (no specific recipient). We persist and log as "Broadcasted".
    public async Task SendPublicNoticeAsync(string title, string message, string audience)
    {
        var notifId = ObjectId.GenerateNewId().ToString();

        var notificationDoc = new NotificationCollection
        {
            NotificationId = notifId,
            Type = "public-notice",
            Title = title,
            Message = message,
            RecipientEmail = string.IsNullOrWhiteSpace(audience) ? "all@uniwa-stls" : audience,
            Status = "Broadcasted",
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("{Tag} Publish public notice {Title} → {Audience}", ServiceTag, title, notificationDoc.RecipientEmail);

        // Persist first
        await _notificationRepository.InsertAsync(notificationDoc);

        // Publish an event for subscribers (UI, other services)
        await _publisher.PublishPublicNoticeAsync(notifId, title, message, notificationDoc.RecipientEmail);

        // Delivery log
        var delivery = new DeliveryLogCollection
        {
            DeliveryId = ObjectId.GenerateNewId().ToString(),
            NotificationId = notifId,
            Status = "Broadcasted",
            DeliveredAt = DateTime.UtcNow,
            DeliveryMethod = "Email" // or "Broadcast"
        };
        await _deliveryLogRepository.InsertAsync(delivery);
    }

    // [POST] /api/notifications (generic DTO entry point)
    public async Task SendNotificationAsync(NotificationDto notification)
    {
        _logger.LogInformation("{Tag} SendNotificationAsync(Type={Type}, Email={Email})",
            ServiceTag, notification.Type, notification.RecipientEmail);

        // We don’t store/track userId in Mongo; business takes an int to preserve upstream identity
        // If you need true user linkage, extend NotificationCollection with a UserId field.
        var fallbackUserId = 0;
        await SendUserNotificationAsync(
            fallbackUserId,
            notification.RecipientEmail,
            notification.Message,
            notification.Type
        );
    }

    // [GET] /api/notifications/public
    public async Task<IEnumerable<NotificationDto>> GetPublicNoticesAsync()
    {
        _logger.LogInformation("{Tag} GetPublicNoticesAsync()", ServiceTag);

        var items = await _notificationRepository.GetBroadcastedAsync();
        var mapped = _mapper.Map<IEnumerable<NotificationDto>>(items);
        return mapped.OrderByDescending(n => n.CreatedAt);
    }

    // [GET] /api/notifications/recipient/{email}
    // We don’t have a repo method for email; use pending + broadcasted and filter in-memory.
    public async Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail)
    {
        _logger.LogInformation("{Tag} GetNotificationsByRecipientEmailAsync({Email})", ServiceTag, recipientEmail);

        var pending = await _notificationRepository.GetPendingAsync();
        var broadcasted = await _notificationRepository.GetBroadcastedAsync();

        var all = pending.Concat(broadcasted)
                         .Where(n => string.Equals(n.RecipientEmail, recipientEmail, StringComparison.OrdinalIgnoreCase));

        // Delivery logs don’t carry RecipientEmail/IsRead in current schema, so IsRead stays default(false)
        return _mapper.Map<IEnumerable<NotificationDto>>(all)
                      .OrderByDescending(n => n.CreatedAt);
    }

    // [GET] /api/notifications
    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
    {
        _logger.LogInformation("{Tag} GetAllNotificationsAsync()", ServiceTag);

        var pending = await _notificationRepository.GetPendingAsync();
        var broadcasted = await _notificationRepository.GetBroadcastedAsync();
        var all = pending.Concat(broadcasted).OrderByDescending(n => n.CreatedAt);

        return _mapper.Map<IEnumerable<NotificationDto>>(all);
    }

    // [PATCH] /api/notifications/{notificationId}/read
    public async Task MarkAsReadAsync(string notificationId, string email)
    {
        _logger.LogInformation("[BUSINESS][NOTIFY] Mark notification {Id} read for {Email}", notificationId, email);
        await _deliveryLogRepository.MarkAsReadAsync(notificationId, email);
    }

    // [PATCH] /api/notifications/read-all
    public async Task MarkAllAsReadAsync(string email)
    {
        _logger.LogInformation("[BUSINESS][NOTIFY] Mark all notifications read for {Email}", email);
        await _deliveryLogRepository.MarkAllAsReadAsync(email);
    }

}
