using AutoMapper;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Features.SocialNetwork.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Application.Mapping
{
    /// <summary>
    /// AutoMapper profile for Application layer mappings
    /// Handles generic mappings for Value Objects and domain aggregates to result DTOs
    /// Uses generic methods to reduce code duplication and improve maintainability
    /// </summary>
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            ConfigureValueObjectMappings();
            ConfigureUserMappings();
        }

        /// <summary>
        /// Configures mappings for Value Objects used across the application
        /// </summary>
        private void ConfigureValueObjectMappings()
        {
            // FullName Value Object mappings
            CreateMap<FullName, string>().ConvertUsing(vo => vo != null ? $"{vo.FirstName} {vo.LastName}".Trim() : string.Empty);

            // Email Value Object mappings
            CreateMap<Email, string>().ConvertUsing(vo => vo != null ? vo.Value : string.Empty);

            // PhoneNumber Value Object mappings
            CreateMap<PhoneNumber, string>().ConvertUsing(vo => vo != null ? vo.Value : string.Empty);
        }

        /// <summary>
        /// Configures User aggregate mappings to various DTOs
        /// </summary>
        private void ConfigureUserMappings()
        {
            // User to GetUserDto mapping (for single user queries)
            CreateMap<User, GetUserDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.GetFullName()))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Latitude : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Longitude : (double?)null));

            // User to GetAllUsersDto mapping (for user list queries)
            CreateMap<User, GetAllUsersDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.GetFullName()))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Latitude : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Longitude : (double?)null));

            // User to NearbyUserDto mapping (for social network queries)
            CreateMap<User, NearbyUserDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.GetFullName()))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Latitude : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Longitude : (double?)null));

            // User to AuthenticatedUserDto mapping (for authentication responses)
            CreateMap<User, AuthenticatedUserDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.GetFullName()))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Latitude : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Longitude : (double?)null));

            // User to RegisterUserInfoDto mapping (for registration responses)
            CreateMap<User, RegisterUserInfoDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FullName.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.FullName.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.GetFullName()))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Latitude : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location != null ? src.Location.Longitude : (double?)null));

            // Authentication DTOs mappings (with AccessToken and nested user data)
            CreateMap<User, AuthenticationDto>()
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore()) // Will be set manually after mapping
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
        }
    }
}
