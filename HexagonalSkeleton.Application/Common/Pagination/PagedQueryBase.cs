using MediatR;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Application.Common.Pagination
{    /// <summary>
    /// Base interface for paginated queries
    /// Provides common pagination parameters for all paginated operations
    /// </summary>
    public interface IPagedQuery
    {
        int PageNumber { get; }
        int PageSize { get; }
        string? SearchTerm { get; }
        string? SortBy { get; }
        string SortDirection { get; }
    }

    /// <summary>
    /// Base class for paginated queries with validation and common behavior
    /// Includes sorting capabilities
    /// </summary>
    public abstract class PagedQuery : IPagedQuery
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;
        private const string DefaultSortDirection = "asc";

        public int PageNumber { get; }
        public int PageSize { get; }
        public string? SearchTerm { get; }
        public string? SortBy { get; }
        public string SortDirection { get; }

        protected PagedQuery(int? pageNumber = null, int? pageSize = null, string? searchTerm = null, string? sortBy = null, string? sortDirection = null)
        {
            PageNumber = pageNumber ?? DefaultPageNumber;
            PageSize = pageSize ?? DefaultPageSize;
            SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
            SortBy = string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim();
            SortDirection = ValidateSortDirection(sortDirection?.Trim()) && !string.IsNullOrWhiteSpace(sortDirection)
                            ? sortDirection.Trim().ToLowerInvariant()
                            : DefaultSortDirection;
        }/// <summary>
         /// Creates domain pagination parameters from query
         /// </summary>
        public PaginationParams ToPaginationParams()
        {
            return PaginationParams.Create(PageNumber, PageSize, SortBy, SortDirection);
        }

        /// <summary>
        /// Validates pagination parameters
        /// Can be overridden for custom validation rules
        /// </summary>
        public virtual bool IsValid()
        {
            return PageNumber > 0 && 
                   PageSize > 0 && 
                   PageSize <= PaginationParams.MaxPageSize &&
                   ValidateSortDirection(SortDirection);
        }

        /// <summary>
        /// Validates sort direction parameter
        /// </summary>
        private static bool ValidateSortDirection(string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortDirection))
                return true;
                
            var normalized = sortDirection.Trim().ToLowerInvariant();
            return normalized == "asc" || normalized == "desc";
        }
    }
}
