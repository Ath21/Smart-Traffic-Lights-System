/*
 * UserStore.UserStoreProfile
 *
 * This class is responsible for defining the AutoMapper profile for the UserStore application.
 * It contains mappings between different data transfer objects (DTOs) and the User entity.
 * The mappings are used for various operations such as user registration, login, and profile updates.
 * The class inherits from AutoMapper's Profile class and defines the mappings in the constructor.
 * The mappings include:
 * - RegisterRequestDto to User: Maps properties from the registration request DTO to the User entity.
 * - User to UserProfileDto: Maps properties from the User entity to the user profile DTO.
 * - Session to LoginResponseDto: Maps properties from the session entity to the login response DTO.
 * - UpdateProfileRequestDto to User: Maps properties from the update profile request DTO to the User entity.
 * The mappings also include custom logic for certain properties, such as generating a new UserId and ignoring the PasswordHash property.
 * The UserStoreProfile class is typically used in the UserService layer of the application to facilitate data transformation
 * between the API and the underlying data model.
 */
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

        // Mapping used for User Profile
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
