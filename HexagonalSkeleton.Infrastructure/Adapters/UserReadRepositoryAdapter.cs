using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Infrastructure.Adapters.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    /// <summary>
    /// Adapter implementing user read repository port
    /// Follows hexagonal architecture by implementing domain ports with infrastructure details
    /// Inherits from base adapter to follow DRY principle
    /// </summary>
    public class UserReadRepositoryAdapter : BaseRepositoryAdapter<UserEntity, User>, IUserReadRepository
    {
        public UserReadRepositoryAdapter(AppDbContext dbContext, IMapper mapper) 
            : base(dbContext, mapper)
        {
        }

        protected override DbSet<UserEntity> GetDbSet() => _dbContext.Users;

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            ValidateId(id);

            var entity = await CreateBaseQuery()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            
            return MapToDomain(entity);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            ValidateStringParameter(email, nameof(email));

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var entity = await CreateBaseQuery()
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);
            
            return MapToDomain(entity);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await CreateBaseQuery()
                .OrderBy(u => u.Id) // Ensure consistent ordering
                .ToListAsync(cancellationToken);
            
            return MapToDomain(entities);
        }

        public async Task<PagedResult<User>> GetPagedAsync(
            PaginationParams pagination, 
            Specification<User>? specification = null, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            var query = CreateBaseQuery();

            // Apply specification if provided
            query = ApplySpecification(query, specification);

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination with consistent ordering
            var entities = await query
                .OrderBy(u => u.Id)
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync(cancellationToken);

            var users = MapToDomain(entities);
            
            return new PagedResult<User>(users, totalCount, pagination);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            ValidateStringParameter(email, nameof(email));

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await CreateBaseQuery()
                .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            ValidateStringParameter(phoneNumber, nameof(phoneNumber));

            var normalizedPhone = phoneNumber.Trim();
            return await CreateBaseQuery()
                .AnyAsync(u => u.PhoneNumber != null && u.PhoneNumber == normalizedPhone, cancellationToken);
        }

        public async Task<List<User>> FindBySpecificationAsync(
            Specification<User> specification, 
            CancellationToken cancellationToken = default)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            var query = CreateBaseQuery();
            query = ApplySpecification(query, specification);

            var entities = await query
                .OrderBy(u => u.Id)
                .ToListAsync(cancellationToken);

            return MapToDomain(entities);
        }

        protected override Expression<Func<UserEntity, bool>> ConvertDomainSpecificationToEntity(Specification<User> specification)
        {
            return specification switch
            {
                UserSearchSpecification searchSpec => ConvertSearchSpecification(searchSpec),
                UserByEmailSpecification emailSpec => ConvertEmailSpecification(emailSpec),
                UserByPhoneNumberSpecification phoneSpec => ConvertPhoneSpecification(phoneSpec),
                UserByIdSpecification idSpec => ConvertIdSpecification(idSpec),
                _ => throw new NotSupportedException($"Specification type {specification.GetType().Name} is not supported")
            };
        }

        private Expression<Func<UserEntity, bool>> ConvertSearchSpecification(UserSearchSpecification spec)
        {
            // Extract the search term from the specification using reflection
            var searchTermField = typeof(UserSearchSpecification)
                .GetField("_searchTerm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var searchTerm = (string)searchTermField!.GetValue(spec)!;

            return entity =>
                (entity.Name != null && entity.Name.ToLower().Contains(searchTerm)) ||
                (entity.Surname != null && entity.Surname.ToLower().Contains(searchTerm)) ||
                (entity.Email != null && entity.Email.ToLower().Contains(searchTerm));
        }

        private Expression<Func<UserEntity, bool>> ConvertEmailSpecification(UserByEmailSpecification spec)
        {
            var emailField = typeof(UserByEmailSpecification)
                .GetField("_email", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var email = (string)emailField!.GetValue(spec)!;

            return entity => entity.Email != null && entity.Email.ToLower() == email;
        }

        private Expression<Func<UserEntity, bool>> ConvertPhoneSpecification(UserByPhoneNumberSpecification spec)
        {
            var phoneField = typeof(UserByPhoneNumberSpecification)
                .GetField("_phoneNumber", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var phone = (string)phoneField!.GetValue(spec)!;

            return entity => entity.PhoneNumber != null && entity.PhoneNumber == phone;
        }

        private Expression<Func<UserEntity, bool>> ConvertIdSpecification(UserByIdSpecification spec)
        {
            var idField = typeof(UserByIdSpecification)
                .GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var id = (int)idField!.GetValue(spec)!;

            return entity => entity.Id == id;        }
    }
}
