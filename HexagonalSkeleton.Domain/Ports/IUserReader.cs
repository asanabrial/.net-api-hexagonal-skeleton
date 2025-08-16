using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Interface segregation: Focused interface for core user retrieval operations
    /// Follows ISP by providing only essential read operations
    /// </summary>
    public interface IUserReader
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
