using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    public class GetUserQuery : IRequest<GetUserDto>
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
