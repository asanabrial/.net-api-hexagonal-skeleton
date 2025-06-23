using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class HardDeleteUserCommand : IRequest<UserDeletionDto>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; }

        // Constructor for compatibility (optional)
        public HardDeleteUserCommand() { }

        // Constructor with parameters for direct instantiation
        public HardDeleteUserCommand(int id)
        {
            Id = id;
        }
    }
}
