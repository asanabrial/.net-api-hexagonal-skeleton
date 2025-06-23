using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Common.Pagination;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Result type alias for GetAllUsersQuery with pagination support.
    /// Simplifies usage while maintaining type safety and domain language.
    /// Follows YAGNI principle - only adds domain-specific naming without additional complexity.
    /// </summary>
    public sealed class GetAllUsersQueryResult
    {
        public IReadOnlyList<UserDto> Users { get; }
        public PaginationMetadata Metadata { get; }
        
        // Convenience properties for backward compatibility
        public int TotalCount => Metadata.TotalCount;
        public int PageNumber => Metadata.PageNumber;
        public int PageSize => Metadata.PageSize;
        public int TotalPages => Metadata.TotalPages;
        public bool HasNextPage => Metadata.HasNextPage;
        public bool HasPreviousPage => Metadata.HasPreviousPage;

        private GetAllUsersQueryResult(PagedQueryResult<UserDto> pagedResult)
        {
            Users = pagedResult.Items;
            Metadata = pagedResult.Metadata;
        }

        /// <summary>
        /// Factory method to create result from generic PagedQueryResult
        /// Simplified approach that focuses only on domain naming
        /// </summary>
        public static GetAllUsersQueryResult FromPagedResult(PagedQueryResult<UserDto> pagedResult)
        {
            return new GetAllUsersQueryResult(pagedResult ?? throw new ArgumentNullException(nameof(pagedResult)));
        }
    }
}
