using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for getting all users with pagination and filtering
    /// Encapsulates query parameters following Request-Response pattern
    /// </summary>
    public class GetAllUsersRequest
    {
        /// <summary>
        /// Page number (1-based, default: 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (1-100, default: 10)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Optional search term for filtering by name or email
        /// </summary>
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Optional sorting criteria
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "asc";
    }
}
