using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    public class GetUserQuery(int id) : IRequest<IResult>
    {
        public int Id { get; set; } = id;
    }
}
