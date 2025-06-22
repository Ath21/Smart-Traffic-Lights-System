using System;
using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Models;

namespace NotificationStore;

public class NotificationStoreProfile : Profile
{
    public NotificationStoreProfile()
    {
        CreateMap<NotificationDto, Notification>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "sent"));

        CreateMap<Notification, NotificationDto>();
    }
}
