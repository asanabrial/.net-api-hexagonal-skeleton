using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class SoftDeleteUserCommand : IRequest<UserDeletionDto>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; }

        // Constructor for compatibility (optional)
        public SoftDeleteUserCommand() { }

        // Constructor with parameters for direct instantiation
        public SoftDeleteUserCommand(int id)
        {
            Id = id;
        }
    }
}
