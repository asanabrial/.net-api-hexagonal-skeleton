using AutoMapper;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;

namespace HexagonalSkeleton.API.Mapping
{
    /// <summary>
    /// AutoMapper profile for API layer mappings
    /// Maps API Models to Application Commands/Queries and vice versa
    /// </summary>
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            // Auth Models → Application Commands/Queries
            CreateMap<LoginRequest, LoginCommand>();            // User Models → Application Commands
            CreateMap<CreateUserRequest, RegisterUserCommand>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName));
            
            CreateMap<UpdateUserRequest, UpdateUserCommand>();
            CreateMap<UpdateProfileRequest, UpdateProfileUserCommand>();            // Application Results → API Responses
            CreateMap<LoginCommandResult, LoginResponse>();
            CreateMap<RegisterUserCommandResult, LoginResponse>();
            CreateMap<GetUserQueryResult, UserResponse>();
            CreateMap<UpdateUserCommandResult, UserResponse>();
            CreateMap<UpdateProfileUserCommandResult, UserResponse>();
            CreateMap<HardDeleteUserCommandResult, DeleteUserResponse>();
            CreateMap<SoftDeleteUserCommandResult, DeleteUserResponse>();
            CreateMap<GetAllUsersQueryResult, UsersResponse>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Users));
            CreateMap<UserDto, UserResponse>();
        }
    }
}
