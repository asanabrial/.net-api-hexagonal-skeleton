using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    public class UserReadRepositoryAdapter : IUserReadRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserReadRepositoryAdapter(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            
            return _mapper.Map<User>(entity);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            
            return _mapper.Map<User>(entity);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            
            return _mapper.Map<List<User>>(entities);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
        }
    }
}
