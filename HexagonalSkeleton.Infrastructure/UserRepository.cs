using AutoMapper;
using HexagonalSkeleton.CommonCore.Data.Repository;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Infrastructure
{
    public class UserRepository(AppDbContext dbContext, IMapper mapper) : GenericRepository<UserEntity>(dbContext), IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
            => mapper.Map<User>(await FindOneAsync(where: e => e.Email == email, cancellationToken: cancellationToken));
        

        public async Task SetLastLogin(int userId, CancellationToken cancellationToken)
        {
            var e = new UserEntity() { LastLogin = DateTime.Now, Id = userId };
            await Update(e, p => p.LastLogin);
        }

        public async Task CreateUserAsync(User entity, CancellationToken cancellationToken = default)
            => await CreateAsync(entity: mapper.Map<UserEntity>(entity), cancellationToken: cancellationToken);

        public Task UpdateUser(User entity)
            => Update(mapper.Map<UserEntity>(entity));

        public Task UpdateProfileUser(User entity)
            => Update(
                entity: mapper.Map<UserEntity>(entity),
                e => e.AboutMe,
                e => e.Birthdate,
                e => e.Name,
                e => e.Surname);
        public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
            => mapper.Map<User>(await FindOneAsync(
                id: id,
                cancellationToken: cancellationToken));

        public async Task<User?> GetTrackedUserByIdAsync(int id, CancellationToken cancellationToken = default)
            => mapper.Map<User>(await FindOneAsync(
                id: id,
                tracking: true,
                cancellationToken: cancellationToken));
        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
            => mapper.Map<List<User>>(await FindAllAsync(cancellationToken: cancellationToken));

        public async Task SoftDeleteUser(int id)
            => await SoftDelete(id: id);

        public async Task HardDeleteUser(int id)
            => await HardDelete(id: id);
    }
}
