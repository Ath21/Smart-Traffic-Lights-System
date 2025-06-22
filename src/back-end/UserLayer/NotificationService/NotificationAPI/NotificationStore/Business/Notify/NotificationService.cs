using System;
using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Models;
using NotificationStore.Repository;

namespace NotificationStore.Business.Notify;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // GET: /API/Notification/GetByRecipient?recipientId=Guid
    public Task<List<NotificationDto?>> GetByRecipientAsync(Guid recipientId)
    {
        var notifications = _repository.GetAsync(recipientId);
        return notifications.ContinueWith(task => _mapper.Map<List<NotificationDto?>>(task.Result));
    }

    // POST: /API/Notification/Create
    public async Task CreateAsync(NotificationDto notification)
    {
        var notificationModel = _mapper.Map<Notification>(notification);
        await _repository.CreateAsync(notificationModel);
    }
}
