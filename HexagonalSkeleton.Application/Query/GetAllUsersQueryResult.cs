using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Result for GetAllUsersQuery operation with pagination support.
    /// Uses composition for better flexibility and maintainability
    /// </summary>
    public class GetAllUsersQueryResult
    {
        /// <summary>
        /// List of users for current page
        /// </summary>
        public IReadOnlyList<UserDto> Users { get; }

        /// <summary>
        /// Pagination metadata
        /// </summary>
        public PaginationMetadata Metadata { get; }

        /// <summary>
        /// Constructor with pagination data
        /// </summary>
        public GetAllUsersQueryResult(IEnumerable<UserDto> users, PaginationMetadata metadata)
        {
            Users = users?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(users));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Convenience properties for backward compatibility
        /// </summary>
        public int TotalCount => Metadata.TotalCount;
        public int PageNumber => Metadata.PageNumber;
        public int PageSize => Metadata.PageSize;
        public int TotalPages => Metadata.TotalPages;
        public bool HasNextPage => Metadata.HasNextPage;
        public bool HasPreviousPage => Metadata.HasPreviousPage;

        /// <summary>
        /// Factory method to create result from domain PagedResult
        /// </summary>
        public static GetAllUsersQueryResult FromDomain<TDomain>(
            PagedResult<TDomain> domainResult, 
            IEnumerable<UserDto> userDtos)
        {
            var metadata = PaginationMetadata.FromDomain(domainResult);
            return new GetAllUsersQueryResult(userDtos, metadata);        }
    }
}
