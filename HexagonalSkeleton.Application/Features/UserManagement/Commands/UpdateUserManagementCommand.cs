using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{    
    /// <summary>
    /// Command to update user information in the management context
    /// Handles administrative user updates with validation
    /// </summary>
    public class UpdateUserManagementCommand : IRequest<UpdateUserDto>
    {
        public required Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime Birthdate { get; set; }
        public required string PhoneNumber { get; set; }
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }
        public required string AboutMe { get; set; }
    }
}
