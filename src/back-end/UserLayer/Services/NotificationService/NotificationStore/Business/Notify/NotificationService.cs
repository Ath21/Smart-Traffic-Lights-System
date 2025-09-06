using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Business.Email;
using NotificationStore.Models.Dtos;
using NotificationStore.Publishers.Notifications;
using NotificationData.Repositories.Notifications;
using NotificationData.Repositories.DeliveryLogs;

namespace NotificationStore.Business.Notify;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDeliveryLogRepository _deliveryLogRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationPublisher _publisher;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationService> _logger;

    private const string ServiceTag = "[" + nameof(NotificationService) + "]";

    public NotificationService(
        INotificationRepository notificationRepository,
        IDeliveryLogRepository deliveryLogRepository,
        IMapper mapper,
        IEmailService emailService,
        INotificationPublisher publisher,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _deliveryLogRepository = deliveryLogRepository;
        _mapper = mapper;
        _emailService = emailService;
        _publisher = publisher;
        _logger = logger;
    }

    // [POST]   /api/notifications/send
    public async Task SendUserNotificationAsync(Guid userId, string email, string message, string type)
    {
        var notifId = Guid.NewGuid();

        var entity = new Notification
        {
            NotificationId = notifId,
            Type = type,
            Title = $"{type} Notification",
            Message = message,
            TargetAudience = userId.ToString(),
            RecipientEmail = email,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("{Tag} Creating user notification {NotificationId} for {Email}",
            ServiceTag, notifId, email);

        await _notificationRepository.InsertAsync(entity);

        _logger.LogInformation("{Tag} Publishing user alert for {UserId}, {Email}",
            ServiceTag, userId, email);

        await _publisher.PublishUserAlertAsync(userId, email, type, message);

        var log = new DeliveryLog
        {
            DeliveryId = Guid.NewGuid(),
            NotificationId = notifId,
            UserId = userId,
            RecipientEmail = email,
            Status = "Sent",
            SentAt = DateTime.UtcNow
        };

        await _deliveryLogRepository.InsertAsync(log);

        if (!string.IsNullOrWhiteSpace(email))
        {
            _logger.LogInformation("{Tag} Sending email to {Email}", ServiceTag, email);
            await _emailService.SendEmailAsync(email, $"[Notification] {type}", message);
        }

        await _notificationRepository.UpdateStatusAsync(notifId, "Sent");
    }

    public async Task SendNotificationAsync(NotificationDto notification)
    {
        var userId = Guid.TryParse(notification.TargetAudience, out var uid) ? uid : Guid.Empty;

        _logger.LogInformation("{Tag} Mapping legacy DTO to SendUserNotificationAsync", ServiceTag);

        await SendUserNotificationAsync(
            userId,
            notification.RecipientEmail,
            notification.Message,
            notification.Type
        );
    }

    // [POST]   /api/notifications/public-notice
    public async Task SendPublicNoticeAsync(string title, string message, string audience)
    {
        var notifId = Guid.NewGuid();

        _logger.LogInformation("{Tag} Publishing public notice {Title} for {Audience}",
            ServiceTag, title, audience);

        await _publisher.PublishPublicNoticeAsync(notifId, title, message, audience);

        var log = new DeliveryLog
        {
            DeliveryId = Guid.NewGuid(),
            NotificationId = notifId,
            UserId = Guid.Empty,
            RecipientEmail = audience,
            Status = "Broadcasted",
            SentAt = DateTime.UtcNow
        };

        await _deliveryLogRepository.InsertAsync(log);
    }

    // [GET]   /api/notifications/recipient/{email}
    public async Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail)
    {
        _logger.LogInformation("{Tag} Fetching notifications for {Email}", ServiceTag, recipientEmail);

        var logs = await _deliveryLogRepository.GetByRecipientEmailAsync(recipientEmail);
        var notificationIds = logs.Select(l => l.NotificationId).Distinct();

        var results = new List<NotificationDto>();

        foreach (var id in notificationIds)
        {
            var notif = await _notificationRepository.GetByIdAsync(id);
            var log = logs.FirstOrDefault(l => l.NotificationId == id);

            if (notif != null && log != null)
            {
                results.Add(new NotificationDto
                {
                    NotificationId = notif.NotificationId,
                    Type = notif.Type,
                    Title = notif.Title,
                    Message = notif.Message,
                    RecipientEmail = notif.RecipientEmail,
                    Status = notif.Status,
                    CreatedAt = notif.CreatedAt,
                    IsRead = log.IsRead
                });
            }
        }

        return results;
    }

    // [GET]   /api/notifications/delivery-history/{userId}
    public async Task<IEnumerable<DeliveryLogDto>> GetDeliveryHistoryAsync(Guid userId)
    {
        _logger.LogInformation("{Tag} Fetching delivery history for User {UserId}", ServiceTag, userId);

        var logs = await _deliveryLogRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<DeliveryLogDto>>(logs);
    }

    // [GET]   /api/notifications
    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
    {
        _logger.LogInformation("{Tag} Fetching all notifications", ServiceTag);

        var notifications = await _notificationRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    // [PATCH] /api/notifications/{notificationId}/read
    public async Task MarkAsReadAsync(Guid notificationId, string email)
    {
        _logger.LogInformation("{Tag} Marking notification {NotificationId} as read for {Email}",
            ServiceTag, notificationId, email);

        await _deliveryLogRepository.MarkAsReadAsync(notificationId, email);
    }

    // [PATCH] /api/notifications/read-all
    public async Task MarkAllAsReadAsync(string email)
    {
        _logger.LogInformation("{Tag} Marking all notifications as read for {Email}", ServiceTag, email);

        await _deliveryLogRepository.MarkAllAsReadAsync(email);
    }
}
