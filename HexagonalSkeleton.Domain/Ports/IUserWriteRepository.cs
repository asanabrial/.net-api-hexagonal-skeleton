using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Port for user write operations (Command side)
    /// Follows DDD principles by working with aggregate roots
    /// </summary>
    public interface IUserWriteRepository
    {
        Task<int> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task SetLastLoginAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
