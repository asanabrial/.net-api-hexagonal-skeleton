using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class SoftDeleteUserCommand(int id) : IRequest<SoftDeleteUserCommandResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;
    }
}
