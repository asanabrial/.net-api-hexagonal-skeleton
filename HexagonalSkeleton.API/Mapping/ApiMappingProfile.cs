using AutoMapper;
using HexagonalSkeleton.API.Models;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Common;
using HexagonalSkeleton.Application.Common.Pagination;
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

            // Mapping for RegisterDto to UserRegistrationResponse
            // Maps only the user information, ignoring authentication token
            CreateMap<RegisterDto, UserRegistrationResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "User registered successfully"));

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
