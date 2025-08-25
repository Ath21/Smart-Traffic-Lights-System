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
        var notifications = await _notificationRepository.GetByRecipientEmailAsync(recipientEmail);
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
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

        // Log delivery
        var log = new DeliveryLog
        {
            DeliveryId = Guid.NewGuid(),
            NotificationId = dto.NotificationId,
            Recipient = email,
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
        await _publisher.PublishPublicNoticeAsync(Guid.NewGuid(), title, message, audience);
    }

    public async Task<IEnumerable<DeliveryLogDto>> GetDeliveryHistoryAsync(Guid userId)
    {
        var logs = await _deliveryLogRepository.GetByRecipientAsync(userId.ToString());
        return _mapper.Map<IEnumerable<DeliveryLogDto>>(logs);
    }
}
