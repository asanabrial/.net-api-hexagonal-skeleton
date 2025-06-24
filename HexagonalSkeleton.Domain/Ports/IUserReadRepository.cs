using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Simple user read repository - easy to understand for newcomers
    /// Only essential methods: get all users or search users
    /// </summary>
    public interface IUserReadRepository
    {
        /// <summary>
        /// Get a single user by ID
        /// </summary>
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get a single user by email (for login/validation)
        /// </summary>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get all users (for admin purposes)
        /// </summary>
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get all active users with pagination (no search filter)
        /// </summary>
        Task<PagedResult<User>> GetUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search users by any field (name, surname, email, phone) with pagination
        /// </summary>
        Task<PagedResult<User>> SearchUsersAsync(PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if user exists by email
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if user exists by phone number
        /// </summary>
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    }
}
