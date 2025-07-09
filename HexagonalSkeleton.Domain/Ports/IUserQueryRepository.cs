using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Query-specific repository for user read operations
    /// Follows CQRS pattern with read-optimized operations
    /// Returns DTOs instead of domain entities for better performance
    /// </summary>
    public interface IUserQueryRepository
    {
        /// <summary>
        /// Get all users with pagination
        /// </summary>
        Task<PagedResult<UserQueryDto>> GetAllAsync(
            PaginationParams paginationParams, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by ID for display purposes
        /// </summary>
        Task<UserQueryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by email for authentication/validation
        /// </summary>
        Task<UserQueryDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user exists by email
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user exists by phone number
        /// </summary>
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search users with text search
        /// </summary>
        Task<PagedResult<UserQueryDto>> SearchAsync(
            string searchTerm, 
            PaginationParams paginationParams, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get users by location radius
        /// </summary>
        Task<PagedResult<UserQueryDto>> GetUsersByLocationAsync(
            double latitude, 
            double longitude, 
            double radiusKm,
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user statistics for analytics
        /// </summary>
        Task<UserStatsDto> GetUserStatsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO for read operations
    /// Optimized for query responses and UI display
    /// </summary>
    public class UserQueryDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? Age { get; set; }
        public double ProfileCompleteness { get; set; }
        public List<string> SearchTerms { get; set; } = new();
    }

    /// <summary>
    /// DTO for user statistics
    /// </summary>
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double AverageAge { get; set; }
        public double AverageProfileCompleteness { get; set; }
    }
}
