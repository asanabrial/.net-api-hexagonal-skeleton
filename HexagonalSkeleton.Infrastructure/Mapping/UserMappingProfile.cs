using AutoMapper;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// AutoMapper profile for mapping between domain entities and infrastructure entities
    /// Follows DDD principles with proper handling of Value Objects and Aggregate boundaries
    /// </summary>
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // UserEntity to Domain User (reconstitution from persistence)
            CreateMap<UserEntity, User>()
                .ConstructUsing(entity => User.Reconstitute(
                    entity.Id,
                    entity.Email ?? string.Empty,
                    entity.Name ?? string.Empty,
                    entity.Surname ?? string.Empty,
                    entity.Birthdate ?? DateTime.MinValue,
                    entity.PhoneNumber ?? string.Empty,
                    entity.Latitude,
                    entity.Longitude,
                    entity.AboutMe ?? string.Empty,
                    entity.PasswordSalt ?? string.Empty,
                    entity.PasswordHash ?? string.Empty,
                    entity.LastLogin,
                    entity.CreatedAt,
                    entity.UpdatedAt,
                    entity.DeletedAt,
                    entity.IsDeleted,
                    entity.ProfileImageName))
                .ForAllMembers(opt => opt.Ignore()); // All mapping handled in ConstructUsing

            // Domain User to UserEntity (persistence mapping)
            CreateMap<User, UserEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude))
                .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src.Birthdate))
                .ForMember(dest => dest.AboutMe, opt => opt.MapFrom(src => src.AboutMe))
                .ForMember(dest => dest.PasswordSalt, opt => opt.MapFrom(src => src.PasswordSalt))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.ProfileImageName, opt => opt.MapFrom(src => src.ProfileImageName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));
        }
    }
}
