using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Models;

namespace NotificationStore;

public class NotificationStoreProfile : Profile
{
    public NotificationStoreProfile()
    {
        // Notification mappings
        CreateMap<NotificationDto, Notification>().ReverseMap();

        // DeliveryLog mappings
        CreateMap<DeliveryLogDto, DeliveryLog>().ReverseMap();
    }
}
