using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Models.Dtos;
using NotificationStore.Models.Responses;

namespace NotificationStore;

public class NotificationStoreProfile : Profile
{
    public NotificationStoreProfile()
    {
        CreateMap<NotificationCollection, NotificationResponse>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotificationId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.RecipientEmail, opt => opt.MapFrom(src => src.RecipientEmail))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsRead, opt => opt.Ignore());

        CreateMap<DeliveryLogCollection, DeliveryLogResponse>()
            .ForMember(dest => dest.DeliveryId, opt => opt.MapFrom(src => src.DeliveryId))
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotificationId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.DeliveryMethod, opt => opt.MapFrom(src => src.DeliveryMethod))
            .ForMember(dest => dest.DeliveredAt, opt => opt.MapFrom(src => src.DeliveredAt));

        CreateMap<DeliveryLogCollection, NotificationResponse>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotificationId))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
            .ForMember(dest => dest.ReadAt, opt => opt.MapFrom(src => src.ReadAt));


        CreateMap<NotificationCollection, NotificationDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotificationId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.RecipientEmail, opt => opt.MapFrom(src => src.RecipientEmail))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsRead, opt => opt.Ignore())
            .ForMember(dest => dest.ReadAt, opt => opt.Ignore());

        CreateMap<DeliveryLogCollection, NotificationDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.NotificationId))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
            .ForMember(dest => dest.ReadAt, opt => opt.MapFrom(src => src.ReadAt));
    }
}
