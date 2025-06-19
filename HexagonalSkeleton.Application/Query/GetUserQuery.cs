using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetUserQuery(int id) : IRequest<GetUserQueryResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;
    }
}
