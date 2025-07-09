using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Interface segregation: Separate interface for user existence checks
    /// Follows ISP by providing only existence-related operations
    /// </summary>
    public interface IUserExistenceChecker
    {
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface segregation: Separate interface for user search operations
    /// Follows ISP by providing only search-related operations
    /// </summary>
    public interface IUserSearchService
    {
        Task<PagedResult<User>> SearchUsersAsync(PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default);
        Task<List<User>> FindNearbyUsersAsync(double latitude, double longitude, double radiusInKm, bool adultOnly, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface segregation: Focused interface for basic user retrieval
    /// Follows ISP by providing only basic read operations
    /// </summary>
    public interface IUserBasicReader
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
