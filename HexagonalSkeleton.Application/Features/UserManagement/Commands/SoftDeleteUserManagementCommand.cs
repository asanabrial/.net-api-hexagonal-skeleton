using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class SoftDeleteUserManagementCommand : IRequest<UserDeletionDto>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public Guid Id { get; set; }

        // Constructor for compatibility (optional)
        public SoftDeleteUserManagementCommand() { }

        // Constructor with parameters for direct instantiation
        public SoftDeleteUserManagementCommand(Guid id)
        {
            Id = id;
        }
    }
}
