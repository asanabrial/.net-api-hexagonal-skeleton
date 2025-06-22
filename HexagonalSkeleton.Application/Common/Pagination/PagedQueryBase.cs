using MediatR;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Application.Common.Pagination
{
    /// <summary>
    /// Base interface for paginated queries
    /// Provides common pagination parameters for all paginated operations
    /// </summary>
    public interface IPagedQuery
    {
        int PageNumber { get; }
        int PageSize { get; }
        string? SearchTerm { get; }
    }

    /// <summary>
    /// Base class for paginated queries with validation and common behavior
    /// </summary>
    public abstract class PagedQuery : IPagedQuery
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

        public int PageNumber { get; }
        public int PageSize { get; }
        public string? SearchTerm { get; }

        protected PagedQuery(int? pageNumber = null, int? pageSize = null, string? searchTerm = null)
        {
            PageNumber = pageNumber ?? DefaultPageNumber;
            PageSize = pageSize ?? DefaultPageSize;
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        }

        /// <summary>
        /// Creates domain pagination parameters from query
        /// </summary>
        public PaginationParams ToPaginationParams()
        {
            return PaginationParams.Create(PageNumber, PageSize);
        }

        /// <summary>
        /// Validates pagination parameters
        /// Can be overridden for custom validation rules
        /// </summary>
        public virtual bool IsValid()
        {
            return PageNumber > 0 && 
                   PageSize > 0 && 
                   PageSize <= PaginationParams.MaxPageSize;
        }
    }
}
