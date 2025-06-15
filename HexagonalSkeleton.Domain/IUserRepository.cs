using HexagonalSkeleton.CommonCore.Data.Repository;

namespace HexagonalSkeleton.Domain
{
    public interface IUserRepository : IGenericRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task SetLastLogin(int userId, CancellationToken cancellationToken);
        Task CreateUserAsync(User entity, CancellationToken cancellationToken = default);
        Task UpdateUser(User entity);
        Task UpdateProfileUser(User entity);
        Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetTrackedUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task SoftDeleteUser(int id);
        Task HardDeleteUser(int id);
    }
}
