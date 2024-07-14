using HexagonalSkeleton.CommonCore.Data.Repository;

namespace HexagonalSkeleton.API.Features.User.Domain
{
    public interface IUserRepository : IGenericRepository<UserEntity>
    {
        Task<UserEntity?> GetByEmail(string email, CancellationToken cancellationToken);
        Task SetLastLogin(string email, CancellationToken cancellationToken);
    }
}
