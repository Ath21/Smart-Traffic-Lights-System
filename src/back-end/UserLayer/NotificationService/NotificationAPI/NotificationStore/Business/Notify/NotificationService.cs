/*
 * NotificationStore.Business.Notify.NotificationService
 *
 * This file is part of the NotificationStore project, which implements the NotificationService class.
 * The NotificationService class provides methods for creating notifications, retrieving all notifications,
 * and getting notifications by recipient email.
 * It uses the INotificationRepository to interact with the data store and the IEmailService to send emails.
 * The class is responsible for encapsulating the business logic related to notifications,
 * allowing for separation of concerns between the API layer and the business logic layer.
 * It includes these methods:
 * - SendNotificationAsync: Creates a new notification and sends an email to the recipient.
 * - GetAllNotificationsAsync: Retrieves all notifications from the data store.
 * - GetNotificationsByRecipientEmailAsync: Retrieves notifications for a specific recipient email.
 */
using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Business.Email;
using NotificationStore.Models;
using NotificationStore.Repository;

namespace NotificationStore.Business.Notify;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public NotificationService(
        INotificationRepository repository,
        IMapper mapper,
        IEmailService emailService)
    {
        _repository = repository;
        _mapper = mapper;
        _emailService = emailService;
    }

    // POST: /API/Notification/SendNotification
    public async Task SendNotificationAsync(NotificationDto notification)
    {
        var notificationModel = _mapper.Map<Notification>(notification);

        await _repository.CreateAsync(notificationModel);

        await _emailService.SendEmailAsync(
            notification.RecipientEmail,
            $"[{notification.Type}] Welcome to PADA Smart Traffic Lights System!",
            $"We received your message : {notification.Message}");
    }

    // GET: /API/Notification/GetAllNotifications
    public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
    {
        var notifications = await _repository.GetAllAsync();

        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    // GET: /API/Notification/GetNotificationsByRecipientEmail?recipientEmail={recipientEmail}
    public async Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientEmailAsync(string recipientEmail)
    {
        var notifications = await _repository.GetByRecipientEmailAsync(recipientEmail);

        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

}
