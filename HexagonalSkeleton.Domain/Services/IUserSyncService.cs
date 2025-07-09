namespace HexagonalSkeleton.Domain.Services
{
    /// <summary>
    /// Interface for user synchronization service
    /// Allows for different implementations in production vs tests
    /// </summary>
    public interface IUserSyncService
    {
        Task SyncUserAsync(User user, CancellationToken cancellationToken = default);
        Task SyncUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default);
        Task RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpdateUserLastLoginAsync(Guid userId, DateTime loginTime, CancellationToken cancellationToken = default);
    }
}
