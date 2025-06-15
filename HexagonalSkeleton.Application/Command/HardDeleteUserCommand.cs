using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class HardDeleteUserCommand(int id) : IRequest<ResultDto>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;
    }
}
