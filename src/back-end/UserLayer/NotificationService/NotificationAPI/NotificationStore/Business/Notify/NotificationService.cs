using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Business.Email;
using NotificationStore.Models;
using NotificationStore.Publishers;
using NotificationStore.Publishers.Notifications;
using NotificationStore.Repositories.DeliveryLogs;
using NotificationStore.Repositories.Notifications;

namespace NotificationStore.Business.Notify;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDeliveryLogRepository _deliveryLogRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationPublisher _publisher;
    private readonly IMapper _mapper;

    public NotificationService(
        INotificationRepository notificationRepository,
        IDeliveryLogRepository deliveryLogRepository,
        IMapper mapper,
        IEmailService emailService,
        INotificationPublisher publisher)
    {
        _notificationRepository = notificationRepository;
        _deliveryLogRepository = deliveryLogRepository;
        _mapper = mapper;
        _emailService = emailService;
        _publisher = publisher;
    }

    // Legacy API (still useful internally)
    public async Task SendNotificationAsync(NotificationDto notification)
    {
        var entity = _mapper.Map<Notification>(notification);

        // Save initial record
        await _notificationRepository.InsertAsync(entity);

        // Send email if provided
        if (!string.IsNullOrWhiteSpace(notification.RecipientEmail))
        {
            await _emailService.SendEmailAsync(
                notification.RecipientEmail,
                $"[Notification] {notification.Type}",
                notification.Message
            );

            // ✅ Log delivery
            var log = new DeliveryLog
            {
                DeliveryId = Guid.NewGuid(),
                NotificationId = entity.NotificationId,
                UserId = Guid.TryParse(notification.TargetAudience, out var uid) ? uid : Guid.Empty,
                RecipientEmail = notification.RecipientEmail,
                Status = "Sent",
                SentAt = DateTime.UtcNow
            };
            await _deliveryLogRepository.InsertAsync(log);
        }

        // Mark sent
        entity.Status = "Sent";
        await _notificationRepository.UpdateStatusAsync(entity.NotificationId, "Sent");
    }


    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
    {
        var notifications = await _notificationRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail)
    {
        // 1. Get delivery logs for this user
        var logs = await _deliveryLogRepository.GetByRecipientEmailAsync(recipientEmail);
        var notificationIds = logs.Select(l => l.NotificationId).Distinct();

        // 2. Fetch notifications by those IDs
        var notifications = new List<Notification>();
        foreach (var id in notificationIds)
        {
            var notif = await _notificationRepository.GetByIdAsync(id);
            if (notif != null)
                notifications.Add(notif);
        }

        // 3. Map to DTO
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    public async Task MarkAsReadAsync(Guid notificationId, string email)
    {
        await _deliveryLogRepository.MarkAsReadAsync(notificationId, email);
    }

    public async Task MarkAllAsReadAsync(string email)
    {
        await _deliveryLogRepository.MarkAllAsReadAsync(email);
    }



    // ================================
    // 🔹 API-driven business methods
    // ================================

    public async Task SendUserNotificationAsync(Guid userId, string email, string message, string type)
    {
        var dto = new NotificationDto
        {
            NotificationId = Guid.NewGuid(),
            Type = type,
            Title = $"{type} Notification",
            Message = message,
            TargetAudience = userId.ToString(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Save in DB
        var entity = _mapper.Map<Notification>(dto);
        await _notificationRepository.InsertAsync(entity);

        // Publish to RabbitMQ
        await _publisher.PublishUserAlertAsync(userId, email, type, message);

        var log = new DeliveryLog
        {
            DeliveryId = Guid.NewGuid(),
            NotificationId = dto.NotificationId,
            UserId = userId,
            RecipientEmail = email,
            Status = "Sent",
            SentAt = DateTime.UtcNow
        };
        await _deliveryLogRepository.InsertAsync(log);


        // Send email (optional)
        if (!string.IsNullOrWhiteSpace(email))
            await _emailService.SendEmailAsync(email, $"[Notification] {type}", message);
    }

    public async Task SendPublicNoticeAsync(string title, string message, string audience)
    {
        var notifId = Guid.NewGuid();

        await _publisher.PublishPublicNoticeAsync(notifId, title, message, audience);

        // ✅ Create DeliveryLog entry for audit
        var log = new DeliveryLog
        {
            DeliveryId = Guid.NewGuid(),
            NotificationId = notifId,
            UserId = Guid.Empty, // not tied to one user
            RecipientEmail = audience, // "all", "operators", etc.
            Status = "Broadcasted",
            SentAt = DateTime.UtcNow
        };
        await _deliveryLogRepository.InsertAsync(log);
    }


    public async Task<IEnumerable<DeliveryLogDto>> GetDeliveryHistoryAsync(Guid userId)
    {
        var logs = await _deliveryLogRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<DeliveryLogDto>>(logs);
    }


}
