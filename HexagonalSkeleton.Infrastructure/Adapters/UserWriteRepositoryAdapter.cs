using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    public class UserWriteRepositoryAdapter : IUserWriteRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserWriteRepositoryAdapter(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<UserEntity>(user);
            _dbContext.Users.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<UserEntity>(user);
            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(user.Id);
            if (entity != null)
            {
                entity.Name = user.Name;
                entity.Surname = user.Surname;
                entity.Birthdate = user.Birthdate;
                entity.AboutMe = user.AboutMe;
                
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(id);
            if (entity != null)
            {
                _dbContext.Users.Remove(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }        public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetLastLoginAsync(int userId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(userId);
            if (entity != null)
            {
                entity.LastLogin = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<User?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(id);
            return _mapper.Map<User>(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
