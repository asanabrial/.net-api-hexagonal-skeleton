using AutoMapper;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;

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
        {            // Solo mapeos que requieren configuración especial
            CreateMap<GetAllUsersQueryResult, UsersResponse>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Users))
                .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount))
                .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));// Mapeos especiales para Login/Register con estructura anidada
            CreateMap<LoginCommandResult, LoginResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => "Bearer"))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn)) // Use real expiration time
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true));            
            
            CreateMap<RegisterUserCommandResult, LoginResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => "Bearer"))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn)) // Use real expiration time
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true));
                
            // Mapeo de UserInfoResult a UserInfoResponse de la API
            CreateMap<UserInfoResult, HexagonalSkeleton.API.Models.Auth.UserInfoResponse>();

            // Mapeos especiales para Delete que requieren configuración de campos
            CreateMap<HardDeleteUserCommandResult, DeleteUserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionType, opt => opt.MapFrom(src => "Hard"));

            CreateMap<SoftDeleteUserCommandResult, DeleteUserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionType, opt => opt.MapFrom(src => "Soft"));
        }
    }
}
