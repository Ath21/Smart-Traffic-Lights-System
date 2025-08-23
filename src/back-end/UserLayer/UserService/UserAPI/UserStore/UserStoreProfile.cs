using AutoMapper;
using UserData.Entities;
using UserStore.Models;

namespace UserStore;

public class UserStoreProfile : Profile
{
    public UserStoreProfile()
    {
        // User ↔ UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // AuditLog ↔ AuditLogDto
        CreateMap<AuditLog, AuditLogDto>();

        // Session ↔ SessionDto
        CreateMap<Session, SessionDto>();
    }
}
