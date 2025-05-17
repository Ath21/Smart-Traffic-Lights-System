using System;
using AutoMapper;
using UserData.Entities;
using UserStore.Models;

namespace UserStore;

public class UserStoreProfile : Profile
{
    public UserStoreProfile()
    {
        // Mapping used for User registration
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active"))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => "User"))
        ;

        // Mappig used for User Profile
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
        ;

        // Mapping used for User Login
        CreateMap<Session, LoginResponseDto>();

        // Mapping used for User Update Profile
        CreateMap<UpdateProfileRequestDto, User>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
        ;
    }
}
