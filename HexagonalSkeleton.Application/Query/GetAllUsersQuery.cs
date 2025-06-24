using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Query to get all users with pagination support
    /// Simplified to use generic PagedQueryResult directly
    /// Inherits from PagedQuery for consistent pagination behavior
    /// </summary>
    public class GetAllUsersQuery : PagedQuery, IRequest<PagedQueryResult<UserDto>>
    {
        /// <summary>
        /// Constructor for parameterless instantiation
        /// </summary>
        public GetAllUsersQuery() : base() { }

        /// <summary>
        /// Constructor with pagination and sorting parameters
        /// </summary>
        public GetAllUsersQuery(
            int? pageNumber = null,
            int? pageSize = null,
            string? searchTerm = null,
            string? sortBy = null,
            string? sortDirection = null)
            : base(pageNumber, pageSize, searchTerm, sortBy, sortDirection)
        {
        }
    }
}
