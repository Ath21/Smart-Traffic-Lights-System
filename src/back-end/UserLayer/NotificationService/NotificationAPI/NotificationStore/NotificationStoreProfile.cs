using AutoMapper;
using NotificationData.Collections;
using NotificationStore.Models.Dtos;
using NotificationStore.Models.Requests;
using NotificationStore.Models.Responses;

namespace NotificationStore;

public class NotificationStoreProfile : Profile
{
    public NotificationStoreProfile()
    {
        // Collections <-> DTOs
        CreateMap<Notification, NotificationDto>().ReverseMap();
        CreateMap<DeliveryLog, DeliveryLogDto>().ReverseMap();

        // DTO -> Response
        CreateMap<NotificationDto, NotificationResponse>();
        CreateMap<DeliveryLogDto, DeliveryLogResponse>();

        // Requests -> DTOs
        CreateMap<SendNotificationRequest, NotificationDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => $"{src.Type} Notification"))
            .ForMember(dest => dest.TargetAudience, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pending"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<PublicNoticeRequest, NotificationDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.RecipientEmail, opt => opt.Ignore())
            .ForMember(dest => dest.TargetAudience, opt => opt.MapFrom(src => src.TargetAudience))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pending"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
