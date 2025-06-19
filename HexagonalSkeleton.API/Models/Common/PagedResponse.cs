namespace HexagonalSkeleton.API.Models.Common
{
    /// <summary>
    /// Generic paginated response for list operations
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class PagedResponse<T> : BaseApiResponse
    {
        /// <summary>
        /// List of items for current page
        /// </summary>
        public IEnumerable<T> Data { get; set; } = [];

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Indicates if there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }
}
