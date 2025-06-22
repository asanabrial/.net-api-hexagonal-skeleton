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
        {
            // Solo mapeos que requieren configuración especial
            CreateMap<GetAllUsersQueryResult, UsersResponse>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Users));            // Mapeos especiales para Login/Register que requieren UserInfo  
            CreateMap<LoginCommandResult, LoginResponse>()
                .ForMember(dest => dest.User, opt => opt.Ignore());            CreateMap<RegisterUserCommandResult, LoginResponse>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => "Bearer"))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => true));
                
            // Mapeo del resultado de registro a UserInfo
            CreateMap<RegisterUserCommandResult, UserInfo>();

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
