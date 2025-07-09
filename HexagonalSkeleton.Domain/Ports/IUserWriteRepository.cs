using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Port for user write operations (Command side)
    /// Follows DDD principles by working with aggregate roots
    /// </summary>
    public interface IUserWriteRepository
    {
        Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task SetLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get user by email - Used for authentication where we need password hash and salt
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
