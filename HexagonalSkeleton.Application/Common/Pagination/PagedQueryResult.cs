using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Application.Common.Pagination
{
    /// <summary>
    /// Generic paginated query result for application layer
    /// Simplified implementation following YAGNI and DRY principles
    /// </summary>
    public sealed class PagedQueryResult<TDto>
    {
        public IReadOnlyList<TDto> Items { get; }
        public PaginationMetadata Metadata { get; }

        private PagedQueryResult(IEnumerable<TDto> items, PaginationMetadata metadata)
        {
            Items = items?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(items));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Factory method to create PagedQueryResult from domain PagedResult
        /// Simplified Anti-Corruption Layer between domain and application
        /// </summary>
        public static PagedQueryResult<TDto> FromDomain<TDomain>(
            PagedResult<TDomain> domainResult, 
            IEnumerable<TDto> dtos)
        {
            if (domainResult == null)
                throw new ArgumentNullException(nameof(domainResult));
            
            if (dtos == null)
                throw new ArgumentNullException(nameof(dtos));

            var metadata = PaginationMetadata.FromDomain(domainResult);
            return new PagedQueryResult<TDto>(dtos, metadata);
        }

        /// <summary>
        /// Factory method to create empty result
        /// </summary>
        public static PagedQueryResult<TDto> Empty(PaginationParams pagination)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

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
    /// Pagination metadata value object
    /// Simplified immutable record following DDD principles
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
        /// Factory method to create from domain PagedResult
        /// </summary>
        public static PaginationMetadata FromDomain<T>(PagedResult<T> domainResult)
        {
            if (domainResult == null)
                throw new ArgumentNullException(nameof(domainResult));

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
