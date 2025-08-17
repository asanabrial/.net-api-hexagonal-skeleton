using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Interface segregation: Separate interface for user search operations
    /// Follows ISP by providing only search-related operations
    /// </summary>
    public interface IUserSearchService
    {
        Task<PagedResult<User>> SearchUsersAsync(PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default);
        Task<List<User>> FindNearbyUsersAsync(double latitude, double longitude, double radiusInKm, bool adultOnly, CancellationToken cancellationToken = default);
    }
}
