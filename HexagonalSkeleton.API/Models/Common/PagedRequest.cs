using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.API.Models.Common
{
    /// <summary>
    /// Base request for paginated queries
    /// </summary>
    public class PagedRequest
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        private int _pageSize = DefaultPageSize;

        /// <summary>
        /// Number of items per page
        /// </summary>
        [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(value, MaxPageSize);
        }

        /// <summary>
        /// Optional search term
        /// </summary>
        [StringLength(100)]
        public string? Search { get; set; }

        /// <summary>
        /// Optional sort field
        /// </summary>
        [StringLength(50)]
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public bool SortDescending { get; set; } = false;
    }
}
