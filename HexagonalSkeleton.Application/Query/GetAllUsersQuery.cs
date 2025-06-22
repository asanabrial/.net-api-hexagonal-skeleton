using HexagonalSkeleton.Application.Common.Pagination;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Query to get all users with pagination support
    /// Inherits from PagedQuery for consistent pagination behavior
    /// </summary>
    public class GetAllUsersQuery : PagedQuery, IRequest<GetAllUsersQueryResult>
    {
        /// <summary>
        /// Constructor for parameterless instantiation
        /// </summary>
        public GetAllUsersQuery() : base() { }

        /// <summary>
        /// Constructor with pagination parameters
        /// </summary>
        public GetAllUsersQuery(int? pageNumber = null, int? pageSize = null, string? searchTerm = null)
            : base(pageNumber, pageSize, searchTerm)
        {
        }
    }
}
