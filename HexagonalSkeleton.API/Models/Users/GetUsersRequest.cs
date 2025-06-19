using HexagonalSkeleton.API.Models.Common;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for getting users with filtering and pagination
    /// </summary>
    public class GetUsersRequest : PagedRequest
    {
        /// <summary>
        /// Filter by user status
        /// </summary>
        public UserStatus? Status { get; set; }

        /// <summary>
        /// Filter by minimum age
        /// </summary>
        public int? MinAge { get; set; }

        /// <summary>
        /// Filter by maximum age
        /// </summary>
        public int? MaxAge { get; set; }

        /// <summary>
        /// Filter by registration date range start
        /// </summary>
        public DateTime? RegisteredAfter { get; set; }

        /// <summary>
        /// Filter by registration date range end
        /// </summary>
        public DateTime? RegisteredBefore { get; set; }
    }

    /// <summary>
    /// User status enumeration
    /// </summary>
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended
    }
}
