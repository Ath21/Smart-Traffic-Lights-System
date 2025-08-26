using AutoMapper;
using UserData.Entities;
using UserStore.Models.Dtos;
using UserStore.Models.Requests;
using UserStore.Models.Responses;

namespace UserStore;

public class UserStoreProfile : Profile
{
    public UserStoreProfile()
    {
        // =======================
        // Entity → DTOs
        // =======================
        CreateMap<User, UserDto>();
        CreateMap<User, UserProfileDto>();
        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<Session, SessionDto>();

        // =======================
        // DTOs → Responses
        // =======================
        CreateMap<UserDto, UserResponse>();
        CreateMap<UserProfileDto, UserProfileResponse>();
        
        // =======================
        // Requests → Entities
        // =======================
        CreateMap<RegisterUserRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // handled in service
            .ForMember(dest => dest.Role, opt => opt.Ignore());        // default role set in service

        CreateMap<UpdateProfileRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        // =======================
        // Requests → DTOs
        // (useful for service layer if needed)
        // =======================
        CreateMap<LoginRequest, UserDto>();
        CreateMap<ResetPasswordRequest, UserDto>();
        CreateMap<NotificationRequest, AuditLogDto>()
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => $"Notification: {src.Type}"))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.LogId, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
    }
}
