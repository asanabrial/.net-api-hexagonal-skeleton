using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Data.Repository;

namespace HexagonalSkeleton.API.Features.User.Infrastructure
{
    public class UserRepository(AppDbContext dbContext) : GenericRepository<UserEntity>(dbContext), IUserRepository
    {
        public async Task<UserEntity?> GetByEmail(string email, CancellationToken cancellationToken)
        {
            return await FindOneAsync(e => e.Email == email, cancellationToken);
        }

        public async Task SetLastLogin(string email, CancellationToken cancellationToken)
        {
            var user = await GetByEmail(email, cancellationToken);
            if (user == null) return;

            user.LastLogin = DateTime.Now;
            await Update(user.Id, user);
        }
    }
}
