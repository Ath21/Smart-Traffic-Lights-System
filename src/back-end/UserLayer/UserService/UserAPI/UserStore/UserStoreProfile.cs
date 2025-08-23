using AutoMapper;
using UserData.Entities;
using UserStore.Models;

namespace UserStore;

public class UserStoreProfile : Profile
{
    public UserStoreProfile()
    {
        // Entity → DTO
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // DTO → Entity (Register, Update)
        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.Role, opt => opt.Ignore()) // handled in service
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<UpdateProfileRequestDto, User>()
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // AuditLogs
        CreateMap<AuditLog, AuditLogDto>();

        // Sessions
        CreateMap<Session, SessionDto>();
    }
}
