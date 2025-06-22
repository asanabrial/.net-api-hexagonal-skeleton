using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Ports
{    /// <summary>
    /// Port for user read operations (Query side)
    /// Follows hexagonal architecture principles by defining contracts without implementation details
    /// </summary>
    public interface IUserReadRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get users with pagination and optional specifications
        /// </summary>
        /// <param name="pagination">Pagination parameters</param>
        /// <param name="specification">Optional specification for filtering</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paged result with users</returns>
        Task<PagedResult<User>> GetPagedAsync(PaginationParams pagination, Specification<User>? specification = null, CancellationToken cancellationToken = default);
        
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Find users by specification
        /// </summary>
        /// <param name="specification">Specification to filter users</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of users matching the specification</returns>
        Task<List<User>> FindBySpecificationAsync(Specification<User> specification, CancellationToken cancellationToken = default);
    }
}
