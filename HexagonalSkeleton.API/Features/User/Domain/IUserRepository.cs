
namespace HexagonalSkeleton.API.Features.User.Domain
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task SetLastLogin(int userId, CancellationToken cancellationToken);
        Task CreateUserAsync(UserEntity entity, CancellationToken cancellationToken = default);
        Task UpdateUser(UserEntity entity);
        Task<UserEntity?> GetProfileUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserEntity?> GetTrackedUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserEntity?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task SoftDeleteUser(int id);
        Task HardDeleteUser(int id);
    }
}
