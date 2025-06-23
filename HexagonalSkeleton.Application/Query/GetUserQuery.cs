using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    public class GetUserQuery : IRequest<UserDto>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; }

        // Constructor for compatibility (optional)
        public GetUserQuery() { }

        // Constructor with parameters for direct instantiation
        public GetUserQuery(int id)
        {
            Id = id;
        }
    }
}
