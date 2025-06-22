using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Application.Common.Pagination
{
    /// <summary>
    /// Generic paginated query result for application layer
    /// Maps domain PagedResult to application DTOs
    /// </summary>
    public class PagedQueryResult<TDto>
    {
        public IReadOnlyList<TDto> Items { get; }
        public PaginationMetadata Metadata { get; }

        public PagedQueryResult(IEnumerable<TDto> items, PaginationMetadata metadata)
        {
            Items = items?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(items));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Creates a PagedQueryResult from domain PagedResult
        /// </summary>
        public static PagedQueryResult<TDto> FromDomain<TDomain>(
            PagedResult<TDomain> domainResult, 
            IEnumerable<TDto> dtos)
        {
            var metadata = new PaginationMetadata(
                domainResult.PageNumber,
                domainResult.PageSize,
                domainResult.TotalCount,
                domainResult.TotalPages,
                domainResult.HasNextPage,
                domainResult.HasPreviousPage
            );

            return new PagedQueryResult<TDto>(dtos, metadata);
        }

        /// <summary>
        /// Creates an empty paginated result
        /// </summary>
        public static PagedQueryResult<TDto> Empty(PaginationParams pagination)
        {
            var metadata = new PaginationMetadata(
                pagination.PageNumber,
                pagination.PageSize,
                0,
                0,
                false,
                false
            );

            return new PagedQueryResult<TDto>(Enumerable.Empty<TDto>(), metadata);
        }
    }

    /// <summary>
    /// Pagination metadata for API responses
    /// Immutable value object for consistent pagination information
    /// </summary>
    public record PaginationMetadata(
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasNextPage,
        bool HasPreviousPage)
    {
        /// <summary>
        /// Creates pagination metadata from domain values
        /// </summary>
        public static PaginationMetadata FromDomain<T>(PagedResult<T> domainResult)
        {
            return new PaginationMetadata(
                domainResult.PageNumber,
                domainResult.PageSize,
                domainResult.TotalCount,
                domainResult.TotalPages,
                domainResult.HasNextPage,
                domainResult.HasPreviousPage
            );
        }
    }
}
