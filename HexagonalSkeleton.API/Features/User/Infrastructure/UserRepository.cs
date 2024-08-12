using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Data.Repository;

namespace HexagonalSkeleton.API.Features.User.Infrastructure
{
    public class UserRepository(AppDbContext dbContext) : GenericRepository<UserEntity>(dbContext), IUserRepository
    {
        public async Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken)
            => await FindOneAsync(where: e => e.Email == email, cancellationToken: cancellationToken);
        

        public async Task SetLastLogin(int userId, CancellationToken cancellationToken)
        {
            var e = new UserEntity() { LastLogin = DateTime.Now, Id = userId };
            await Update(e, p => p.LastLogin);
        }

        public async Task CreateUserAsync(UserEntity entity, CancellationToken cancellationToken = default)
            => await CreateAsync(entity: entity, cancellationToken: cancellationToken);

        public Task UpdateUser(UserEntity entity)
            => Update(entity);

        public async Task<UserEntity?> GetProfileUserByIdAsync(int id, CancellationToken cancellationToken = default)
            => await FindOneAsync(
                id: id,
                cancellationToken: cancellationToken);

        public async Task<UserEntity?> GetTrackedUserByIdAsync(int id, CancellationToken cancellationToken = default)
            => await FindOneAsync(
                id: id,
                tracking: true,
                cancellationToken: cancellationToken);

        public async Task<UserEntity?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
            => await FindOneAsync(
                id: id,
                cancellationToken: cancellationToken);
        public async Task<List<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken = default)
            => await FindAllAsync(cancellationToken: cancellationToken);

        public async Task SoftDeleteUser(int id)
            => await SoftDelete(id: id);

        public async Task HardDeleteUser(int id)
            => await HardDelete(id: id);
    }
}
