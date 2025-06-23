using System;
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

    public NotificationService(INotificationRepository repository, IMapper mapper, IEmailService emailService)
    {
        _repository = repository;
        _mapper = mapper;
        _emailService = emailService;
    }



    // POST: /API/Notification/Create
    public async Task CreateAsync(NotificationDto notification)
    {
        var notificationModel = _mapper.Map<Notification>(notification);

        await _repository.CreateAsync(notificationModel);

        await _emailService.SendEmailAsync(
            notification.RecipientEmail,
            $"[{notification.Type}] Notification",
            notification.Message);
    }

    public async Task<IEnumerable<NotificationDto>> GetAllAsync()
    {
        var notifications = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    public async Task<IEnumerable<NotificationDto>> GetByRecipientEmailAsync(string recipientEmail)
    {
        var notifications = await _repository.GetByRecipientEmailAsync(recipientEmail);
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

}
