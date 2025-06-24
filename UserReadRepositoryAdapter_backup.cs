using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Infrastructure.Adapters.Base;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    /// <summary>
    /// Simple repository implementation - no complex specifications
    /// Easy to understand for any developer joining the project
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
                .OrderBy(u => u.Id)
                .ToListAsync(cancellationToken);
            
            return MapToDomain(entities);
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

            var query = CreateBaseQuery()
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

            var term = searchTerm.Trim().ToLowerInvariant();

            var query = CreateBaseQuery()
                .Where(u => !u.IsDeleted && // Only active users
                       (u.Name != null && u.Name.ToLowerInvariant().Contains(term)) ||
                       (u.Surname != null && u.Surname.ToLowerInvariant().Contains(term)) ||
                       (u.Email != null && u.Email.ToLowerInvariant().Contains(term)) ||
                       (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));

            return await ExecutePagedQuery(query, pagination, cancellationToken);
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

            var users = MapToDomain(entities);
            
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
}        public async Task<PagedResult<User>> GetPagedAsync(
            PaginationParams pagination, 
            ISpecification<User>? specification = null, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            var query = CreateBaseQuery();

            // Check if specification requires client-side evaluation
            if (specification != null && RequiresClientSideEvaluation(specification))
            {
                // For composite specifications that can't be translated to SQL,
                // we need to do client-side evaluation
                
                // First, get all entities (without specification filter) and convert to domain objects
                var allEntities = await query
                    .OrderBy(u => u.Id) // Ensure consistent ordering
                    .ToListAsync(cancellationToken);
                var allUsers = MapToDomain(allEntities);

                // Apply specification client-side
                var filteredUsers = ApplySpecificationClientSide(allUsers, specification).ToList();

                // Get total count after filtering
                var totalCount = filteredUsers.Count;

                // Apply sorting if specified, otherwise default to Id
                var sortedUsers = ApplySortingToDomain(filteredUsers, pagination);

                // Apply pagination to domain objects
                var pagedUsers = sortedUsers
                    .Skip(pagination.Skip)
                    .Take(pagination.Take)
                    .ToList();

                return new PagedResult<User>(pagedUsers, totalCount, pagination);
            }
            else
            {
                // For simple specifications that can be translated to SQL, use database-side filtering
                query = ApplySpecification(query, specification);

                // Get total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting if specified, otherwise default to Id
                query = ApplySorting(query, pagination);

                // Apply pagination
                var entities = await query
                    .Skip(pagination.Skip)
                    .Take(pagination.Take)
                    .ToListAsync(cancellationToken);

                var users = MapToDomain(entities);
                
                return new PagedResult<User>(users, totalCount, pagination);
            }
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
        }        public async Task<List<User>> FindBySpecificationAsync(
            Specification<User> specification, 
            CancellationToken cancellationToken = default)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            // Check if specification requires client-side evaluation
            if (RequiresClientSideEvaluation(specification))
            {
                // For composite specifications, get all data and filter client-side
                var allEntities = await CreateBaseQuery()
                    .OrderBy(u => u.Id) // Keep consistent ordering
                    .ToListAsync(cancellationToken);
                var allUsers = MapToDomain(allEntities);

                // Apply specification client-side
                return ApplySpecificationClientSide(allUsers, specification).ToList();
            }
            else
            {
                // For simple specifications, use database-side filtering
                var query = CreateBaseQuery();
                query = ApplySpecification(query, specification);

                var entities = await query
                    .OrderBy(u => u.Id) // Keep consistent ordering for specifications
                    .ToListAsync(cancellationToken);

                return MapToDomain(entities);
            }
        }        /// <summary>
        /// Find users by specification (ISpecification version)
        /// </summary>
        public async Task<List<User>> FindBySpecificationAsync(
            ISpecification<User> specification, 
            CancellationToken cancellationToken = default)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            // Check if specification requires client-side evaluation
            if (RequiresClientSideEvaluation(specification))
            {
                // For composite specifications, get all data and filter client-side
                var allEntities = await CreateBaseQuery()
                    .OrderBy(u => u.Id) // Keep consistent ordering
                    .ToListAsync(cancellationToken);
                var allUsers = MapToDomain(allEntities);

                // Apply specification client-side
                return ApplySpecificationClientSide(allUsers, specification).ToList();
            }
            else
            {
                // For simple specifications, use database-side filtering
                var query = CreateBaseQuery();
                query = ApplySpecification(query, specification);

                var entities = await query
                    .OrderBy(u => u.Id) // Keep consistent ordering for specifications
                    .ToListAsync(cancellationToken);

                return MapToDomain(entities);
            }
        }        /// <summary>
        /// Count users matching a specification
        /// </summary>
        public async Task<int> CountBySpecificationAsync(
            ISpecification<User> specification, 
            CancellationToken cancellationToken = default)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            // Check if specification requires client-side evaluation
            if (RequiresClientSideEvaluation(specification))
            {
                // For composite specifications, get all data and count client-side
                var allEntities = await CreateBaseQuery()
                    .ToListAsync(cancellationToken);
                var allUsers = MapToDomain(allEntities);

                // Apply specification client-side and count
                return ApplySpecificationClientSide(allUsers, specification).Count();
            }
            else
            {
                // For simple specifications, use database-side counting
                var query = CreateBaseQuery();
                query = ApplySpecification(query, specification);

                return await query.CountAsync(cancellationToken);
            }
        }protected override Expression<Func<UserEntity, bool>> ConvertDomainSpecificationToEntity(Specification<User> specification)
        {
            return specification switch
            {
                UserSearchSpecification searchSpec => ConvertSearchSpecification(searchSpec),
                UserByEmailSpecification emailSpec => ConvertEmailSpecification(emailSpec),
                UserByPhoneNumberSpecification phoneSpec => ConvertPhoneSpecification(phoneSpec),
                UserByIdSpecification idSpec => ConvertIdSpecification(idSpec),
                UserIsNotDeletedSpecification => ConvertIsNotDeletedSpecification(),
                // For composite specifications, we need to evaluate client-side
                // Return a "true" filter and handle it differently in GetPagedAsync
                _ => throw new NotSupportedException($"Composite specification {specification.GetType().Name} requires client-side evaluation")
            };
        }        protected override Expression<Func<UserEntity, bool>> ConvertDomainExpressionToEntity(Expression<Func<User, bool>> domainExpression)
        {
            // For composite specifications that can't be translated to SQL,
            // we throw an exception and handle it at the repository level
            throw new NotSupportedException($"Domain expression requires client-side evaluation and cannot be translated to SQL directly.");
        }

        private Expression<Func<UserEntity, bool>> ConvertIsNotDeletedSpecification()
        {
            return entity => !entity.IsDeleted;
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

            return entity => entity.Id == id;
        }

        /// <summary>
        /// Applies sorting to the query based on pagination parameters
        /// </summary>
        private IQueryable<UserEntity> ApplySorting(IQueryable<UserEntity> query, PaginationParams pagination)
        {
            if (!pagination.HasSorting)
            {
                // Default sorting by Id for consistent pagination
                return query.OrderBy(u => u.Id);
            }

            var sortBy = pagination.SortBy!.ToLowerInvariant();
            var isAscending = pagination.IsAscending;

            return sortBy switch
            {
                "id" => isAscending ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
                "name" => isAscending ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
                "surname" => isAscending ? query.OrderBy(u => u.Surname) : query.OrderByDescending(u => u.Surname),
                "email" => isAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "phonenumber" => isAscending ? query.OrderBy(u => u.PhoneNumber) : query.OrderByDescending(u => u.PhoneNumber),
                "createdat" => isAscending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                "updatedat" => isAscending ? query.OrderBy(u => u.UpdatedAt) : query.OrderByDescending(u => u.UpdatedAt),
                _ => query.OrderBy(u => u.Id) // Fallback to default sorting
            };
        }        /// <summary>
        /// Applies sorting to domain objects based on pagination parameters
        /// </summary>
        private IEnumerable<User> ApplySortingToDomain(IEnumerable<User> users, PaginationParams pagination)
        {
            if (!pagination.HasSorting)
            {
                // Default sorting by Id for consistent pagination
                return users.OrderBy(u => u.Id);
            }

            var sortBy = pagination.SortBy!.ToLowerInvariant();
            var isAscending = pagination.IsAscending;

            return sortBy switch
            {
                "id" => isAscending ? users.OrderBy(u => u.Id) : users.OrderByDescending(u => u.Id),
                "name" => isAscending ? users.OrderBy(u => u.FullName?.FirstName) : users.OrderByDescending(u => u.FullName?.FirstName),
                "surname" => isAscending ? users.OrderBy(u => u.FullName?.LastName) : users.OrderByDescending(u => u.FullName?.LastName),
                "email" => isAscending ? users.OrderBy(u => u.Email?.Value) : users.OrderByDescending(u => u.Email?.Value),
                "phonenumber" => isAscending ? users.OrderBy(u => u.PhoneNumber?.Value) : users.OrderByDescending(u => u.PhoneNumber?.Value),
                "createdat" => isAscending ? users.OrderBy(u => u.CreatedAt) : users.OrderByDescending(u => u.CreatedAt),
                "updatedat" => isAscending ? users.OrderBy(u => u.UpdatedAt) : users.OrderByDescending(u => u.UpdatedAt),
                _ => users.OrderBy(u => u.Id) // Fallback to default sorting
            };
        }

        /// <summary>
        /// Get active users with pagination (no additional filters)
        /// </summary>
        public async Task<PagedResult<User>> GetPagedActiveUsersAsync(
            PaginationParams pagination, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));

            var query = CreateBaseQuery()
                .Where(u => !u.IsDeleted); // Only active users

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, pagination);

            // Apply pagination
            var entities = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync(cancellationToken);

            var users = MapToDomain(entities);
            
            return new PagedResult<User>(users, totalCount, pagination);
        }

        /// <summary>
        /// Get users by search term with pagination
        /// </summary>
        public async Task<PagedResult<User>> GetPagedBySearchAsync(
            PaginationParams pagination, 
            string searchTerm, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));
            
            ValidateStringParameter(searchTerm, nameof(searchTerm));

            var normalizedSearchTerm = searchTerm.Trim().ToLowerInvariant();
            
            var query = CreateBaseQuery()
                .Where(u => !u.IsDeleted && 
                           ((u.Name != null && u.Name.ToLower().Contains(normalizedSearchTerm)) ||
                            (u.Surname != null && u.Surname.ToLower().Contains(normalizedSearchTerm)) ||
                            (u.Email != null && u.Email.ToLower().Contains(normalizedSearchTerm))));

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, pagination);

            // Apply pagination
            var entities = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync(cancellationToken);

            var users = MapToDomain(entities);
            
            return new PagedResult<User>(users, totalCount, pagination);
        }

        /// <summary>
        /// Get users by phone number with pagination
        /// </summary>
        public async Task<PagedResult<User>> GetPagedByPhoneAsync(
            PaginationParams pagination, 
            string phoneNumber, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));
            
            ValidateStringParameter(phoneNumber, nameof(phoneNumber));

            var normalizedPhone = phoneNumber.Trim();
            
            var query = CreateBaseQuery()
                .Where(u => !u.IsDeleted && 
                           u.PhoneNumber != null && 
                           u.PhoneNumber == normalizedPhone);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, pagination);

            // Apply pagination
            var entities = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync(cancellationToken);

            var users = MapToDomain(entities);
            
            return new PagedResult<User>(users, totalCount, pagination);
        }

        /// <summary>
        /// Get users by email with pagination
        /// </summary>
        public async Task<PagedResult<User>> GetPagedByEmailAsync(
            PaginationParams pagination, 
            string email, 
            CancellationToken cancellationToken = default)
        {
            if (pagination == null)
                throw new ArgumentNullException(nameof(pagination));
            
            ValidateStringParameter(email, nameof(email));

            var normalizedEmail = email.Trim().ToLowerInvariant();
            
            var query = CreateBaseQuery()
                .Where(u => !u.IsDeleted && 
                           u.Email != null && 
                           u.Email.ToLower() == normalizedEmail);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, pagination);

            // Apply pagination
            var entities = await query
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .ToListAsync(cancellationToken);

            var users = MapToDomain(entities);
            
            return new PagedResult<User>(users, totalCount, pagination);
        }
    }
}
