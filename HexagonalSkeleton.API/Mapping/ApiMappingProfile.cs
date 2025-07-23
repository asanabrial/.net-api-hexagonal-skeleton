using AutoMapper;
using HexagonalSkeleton.API.Models;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Common;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;

namespace HexagonalSkeleton.API.Mapping
{
    /// <summary>
    /// AutoMapper profile for API layer mappings
    /// Uses attribute-based mapping for automatic configuration
    /// Only contains mappings that require special configuration
    /// </summary>
    public class ApiMappingProfile : Profile
    {        
        public ApiMappingProfile()
        {
            // Mapping for RegisterUserInfoDto to UserInfoResponse
            CreateMap<RegisterUserInfoDto, UserInfoResponse>();

            // Mapping for RegisterUserInfoDto to RegisterUserInfoResponse
            CreateMap<RegisterUserInfoDto, RegisterUserInfoResponse>();

            // Mapping for RegisterDto to UserRegistrationResponse
            // Maps only the user information, ignoring authentication token
            CreateMap<RegisterUserDto, UserRegistrationResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "User registered successfully"));

            // Maps authentication token and user information for login response
            CreateMap<RegisterUserDto, LoginResponse>()
                .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.AccessToken))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => src.TokenType))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // Maps authentication token and user information for authenticated registration response
            CreateMap<RegisterUserDto, AuthenticatedRegistrationResponse>()
                .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.AccessToken))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => src.TokenType))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // Maps UserProfileDto to UserResponse for profile update responses
            CreateMap<UserProfileDto, UserResponse>();

            // Generic mapping for ALL paginated responses - SUPER REUSABLE!
            // Works for any PagedQueryResult<TDto> to PagedResponse<TResponse>
            CreateMap(typeof(PagedQueryResult<>), typeof(PagedResponse<>))
                .ForMember("Data", opt => opt.MapFrom("Items"))
                .ForMember("TotalCount", opt => opt.MapFrom("Metadata.TotalCount"))
                .ForMember("PageNumber", opt => opt.MapFrom("Metadata.PageNumber"))
                .ForMember("PageSize", opt => opt.MapFrom("Metadata.PageSize"));
        }
    }
}
