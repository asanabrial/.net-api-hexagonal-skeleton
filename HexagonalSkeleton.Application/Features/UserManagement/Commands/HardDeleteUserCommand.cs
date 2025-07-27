using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class HardDeleteUserCommand : IRequest<UserDeletionDto>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public Guid Id { get; set; }

        // Constructor for compatibility (optional)
        public HardDeleteUserCommand() { }

        // Constructor with parameters for direct instantiation
        public HardDeleteUserCommand(Guid id)
        {
            Id = id;
        }
    }
}
