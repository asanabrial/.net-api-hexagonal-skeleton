using AutoMapper;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Common;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.API.Mapping
{
    /// <summary>
    /// AutoMapper profile for API layer mappings
    /// Uses attribute-based mapping for automatic configuration
    /// Only contains mappings that require special configuration
    /// </summary>
    public class ApiMappingProfile : Profile
    {        public ApiMappingProfile()
        {            // Generic mapping for ALL paginated responses - SUPER REUSABLE!
            // Works for any PagedQueryResult<TDto> to PagedResponse<TResponse>
            CreateMap(typeof(PagedQueryResult<>), typeof(PagedResponse<>))
                .ForMember("Data", opt => opt.MapFrom("Items"))
                .ForMember("TotalCount", opt => opt.MapFrom("Metadata.TotalCount"))
                .ForMember("PageNumber", opt => opt.MapFrom("Metadata.PageNumber"))
                .ForMember("PageSize", opt => opt.MapFrom("Metadata.PageSize"));            // Specific mapping for UsersResponse which inherits from PagedResponse<UserResponse>

            // Request to Query mappings
            CreateMap<GetAllUsersRequest, GetAllUsersQuery>();            // Mapeos especiales para Login/Register con estructura anidada
            CreateMap<LoginCommandResult, LoginResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => "Bearer"))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn)); // Use real expiration time
            
            CreateMap<RegisterUserCommandResult, LoginResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => "Bearer"))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn)); // Use real expiration time
                
            // Mapeo de UserInfoResult a UserInfoResponse de la API
            CreateMap<UserInfoResult, HexagonalSkeleton.API.Models.Auth.UserInfoResponse>();

            // Mapeos especiales para Delete que requieren configuración de campos
            CreateMap<HardDeleteUserCommandResult, DeleteUserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionType, opt => opt.MapFrom(src => "Hard"));            CreateMap<SoftDeleteUserCommandResult, DeleteUserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())                .ForMember(dest => dest.DeletionType, opt => opt.MapFrom(src => "Soft"));
        }
    }
}
