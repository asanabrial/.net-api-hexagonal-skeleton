using AutoMapper;
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
    {        public ApiMappingProfile()
        {
            // Mapping for RegisterUserInfoDto to UserInfoResponse
            CreateMap<RegisterUserInfoDto, UserInfoResponse>();

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
