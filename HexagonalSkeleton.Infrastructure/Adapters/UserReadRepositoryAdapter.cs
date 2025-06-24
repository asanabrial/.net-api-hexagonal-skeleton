using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    /// <summary>
    /// Simple repository implementation - no complex specifications
    /// Easy to understand for any developer joining the project
    /// </summary>
    public class UserReadRepositoryAdapter : IUserReadRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserReadRepositoryAdapter(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var entity = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            
            return entity != null ? _mapper.Map<User>(entity) : null;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            var normalizedEmail = email.Trim().ToLower();
            var entity = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);
            
            return entity != null ? _mapper.Map<User>(entity) : null;
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.Users
                .OrderBy(u => u.Id)
                .ToListAsync(cancellationToken);
            
            return _mapper.Map<List<User>>(entities);
        }

        /// <summary>
        /// Get all active users with pagination (no search filter)
        /// Super simple - anyone can understand this
        /// </summary>
        public async Task<PagedResult<User>> GetUsersAsync(
            PaginationParams pagination, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            var query = _dbContext.Users
                .Where(u => !u.IsDeleted); // Only active users

            return await ExecutePagedQuery(query, pagination, cancellationToken);
        }

        /// <summary>
        /// Search users by term in name, surname, email, or phone
        /// Super simple query - anyone can understand this
        /// </summary>
        public async Task<PagedResult<User>> SearchUsersAsync(
            PaginationParams pagination,
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

            var term = searchTerm.Trim().ToLower();

            var query = _dbContext.Users
                .Where(u => !u.IsDeleted && // Only active users
                       (u.Name != null && u.Name.ToLower().Contains(term)) ||
                       (u.Surname != null && u.Surname.ToLower().Contains(term)) ||
                       (u.Email != null && u.Email.ToLower().Contains(term)) ||
                       (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));

            return await ExecutePagedQuery(query, pagination, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            var normalizedEmail = email.Trim().ToLower();
            return await _dbContext.Users
                .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

            var normalizedPhone = phoneNumber.Trim();
            return await _dbContext.Users
                .AnyAsync(u => u.PhoneNumber != null && u.PhoneNumber == normalizedPhone, cancellationToken);
        }

        /// <summary>
        /// Helper method to execute paginated queries
        /// Keeps the code DRY and simple
        /// </summary>
        private async Task<PagedResult<User>> ExecutePagedQuery(
            IQueryable<UserEntity> query, 
            PaginationParams pagination, 
            CancellationToken cancellationToken)
        {
            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, pagination);

            // Apply pagination
            var entities = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync(cancellationToken);

            var users = _mapper.Map<List<User>>(entities);
            
            return new PagedResult<User>(users, totalCount, pagination);
        }

        /// <summary>
        /// Simple sorting logic
        /// </summary>
        private IQueryable<UserEntity> ApplySorting(IQueryable<UserEntity> query, PaginationParams pagination)
        {
            if (!pagination.HasSorting)
                return query.OrderBy(u => u.Id);

            var isAscending = pagination.IsAscending;
            var sortBy = pagination.SortBy!.ToLowerInvariant();

            return sortBy switch
            {
                "id" => isAscending ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
                "name" => isAscending ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
                "surname" => isAscending ? query.OrderBy(u => u.Surname) : query.OrderByDescending(u => u.Surname),
                "email" => isAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "phonenumber" => isAscending ? query.OrderBy(u => u.PhoneNumber) : query.OrderByDescending(u => u.PhoneNumber),
                "createdat" => isAscending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                "updatedat" => isAscending ? query.OrderBy(u => u.UpdatedAt) : query.OrderByDescending(u => u.UpdatedAt),
                _ => query.OrderBy(u => u.Id)
            };
        }
    }
}
