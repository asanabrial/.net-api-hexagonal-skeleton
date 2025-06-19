using HexagonalSkeleton.API.Models.Common;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Response model for getting multiple users
    /// </summary>
    public class UsersResponse : PagedResponse<UserResponse>
    {
        /// <summary>
        /// Summary statistics
        /// </summary>
        public UsersSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Summary information about users
    /// </summary>
    public class UsersSummary
    {
        /// <summary>
        /// Total number of active users
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Number of new users this month
        /// </summary>
        public int NewUsersThisMonth { get; set; }

        /// <summary>
        /// Average user age (if birthdate is provided)
        /// </summary>
        public double? AverageAge { get; set; }
    }
}
