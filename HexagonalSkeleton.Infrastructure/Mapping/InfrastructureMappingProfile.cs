using AutoMapper;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Mapping
{
    /// <summary>
    /// AutoMapper profile for Infrastructure layer mappings
    /// Handles the mapping between domain objects and entity objects,
    /// including Value Objects and complex domain mappings
    /// </summary>
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            // DateTime converters - explicit UTC handling for PostgreSQL
            CreateMap<DateTime, DateTime>()
                .ConvertUsing(src => src.Kind == DateTimeKind.Utc ? src : 
                    src.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(src, DateTimeKind.Utc) : 
                    src.ToUniversalTime());
            
            CreateMap<DateTime?, DateTime?>()
                .ConvertUsing(src => !src.HasValue ? null : 
                    src.Value.Kind == DateTimeKind.Utc ? src.Value : 
                    src.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(src.Value, DateTimeKind.Utc) : 
                    src.Value.ToUniversalTime());

            // Email Value Object mappings
            CreateMap<Email, string>().ConvertUsing(vo => vo != null ? vo.Value : default!);
            CreateMap<string, Email>().ConvertUsing(value => !string.IsNullOrEmpty(value) ? new Email(value) : default!);
            
            // PhoneNumber Value Object mappings
            CreateMap<PhoneNumber, string>().ConvertUsing(vo => vo != null ? vo.Value : default!);
            CreateMap<string, PhoneNumber>().ConvertUsing(value => !string.IsNullOrEmpty(value) ? new PhoneNumber(value) : default!);
            
            // UserId Value Object mappings
            CreateMap<UserId, Guid>().ConvertUsing(vo => vo != null ? vo.Value : Guid.Empty);
            CreateMap<Guid, UserId>().ConvertUsing(value => value != Guid.Empty ? new UserId(value) : default!);
            
            // User <-> UserEntity mappings with complex value objects
            CreateMap<User, UserEntity>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value));

            CreateMap<UserEntity, User>()
                .ConstructUsing(src => User.Reconstitute(
                    src.Id,
                    src.Email ?? "",
                    src.Name ?? "",
                    src.Surname ?? "",
                    src.Birthdate ?? DateTime.MinValue,
                    src.PhoneNumber ?? "",                    src.Latitude,
                    src.Longitude,
                    src.AboutMe ?? "",
                    src.PasswordSalt ?? "",
                    src.PasswordHash ?? "",
                    src.LastLogin,
                    src.CreatedAt,
                    src.UpdatedAt,
                    src.DeletedAt,
                    src.IsDeleted,
                    src.ProfileImageName));
        }
    }
}
