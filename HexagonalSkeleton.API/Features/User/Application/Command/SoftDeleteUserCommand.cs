using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class represents a command for logging in a user.
    /// </summary>
    public class SoftDeleteUserCommand(int id) : IRequest<IResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;
    }
}
