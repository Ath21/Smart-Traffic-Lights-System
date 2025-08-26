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
        // Entity → DTOs
        CreateMap<User, UserDto>();
        CreateMap<User, UserProfileDto>();
        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<Session, SessionDto>();

        // Entity → Responses (direct mapping) ✅
        CreateMap<User, UserResponse>();
        CreateMap<User, UserProfileResponse>();

        // DTO → Responses
        CreateMap<UserDto, UserResponse>();
        CreateMap<UserProfileDto, UserProfileResponse>();

        // Requests → Entities
        CreateMap<RegisterUserRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore());

        CreateMap<UpdateProfileRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore());
    }
}
