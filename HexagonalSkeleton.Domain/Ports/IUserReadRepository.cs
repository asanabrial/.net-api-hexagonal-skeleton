using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// User read repository with specification pattern support
    /// Follows hexagonal architecture principles with clean separation of concerns
    /// </summary>
    public interface IUserReadRepository
    {
        /// <summary>
        /// Get a single user by ID
        /// </summary>
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get a single user by ID without filtering deleted users (for management purposes)
        /// </summary>
        Task<User?> GetByIdUnfilteredAsync(Guid id, CancellationToken cancellationToken = default);
        
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
        /// Get users that satisfy the given specification with pagination
        /// This is the core method for specification pattern implementation
        /// </summary>
        Task<PagedResult<User>> GetUsersAsync(ISpecification<User> specification, PaginationParams pagination, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get all users that satisfy the given specification (without pagination)
        /// Use with caution - consider pagination for large datasets
        /// </summary>
        Task<List<User>> GetUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Count users that satisfy the given specification
        /// Useful for getting total count without fetching data
        /// </summary>
        Task<int> CountUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if any user satisfies the given specification
        /// More efficient than counting when you only need to know if any exist
        /// </summary>
        Task<bool> AnyUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default);
        
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
