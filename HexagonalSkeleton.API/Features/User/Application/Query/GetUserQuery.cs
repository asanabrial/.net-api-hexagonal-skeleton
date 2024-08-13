using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    public class GetUserQuery(int id) : IRequest<IResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;
    }
}
