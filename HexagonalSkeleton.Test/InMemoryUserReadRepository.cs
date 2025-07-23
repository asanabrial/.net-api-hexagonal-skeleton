using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Repositorio en memoria para tests que mantiene sincronización CQRS
    /// entre instancias usando estado compartido thread-safe
    /// </summary>
    public class InMemoryUserReadRepository : IUserReadRepository
    {
        // Colección estática compartida para sincronización CQRS entre tests
        private static readonly List<User> _sharedUsers = new();
        private static readonly object _sharedLock = new();

        #region Consultas básicas
        
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                var user = _sharedUsers.FirstOrDefault(u => u.Id == id);
                return Task.FromResult(user);
            }
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                var user = _sharedUsers.FirstOrDefault(u => u.Email.Value == email);
                return Task.FromResult(user);
            }
        }

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.ToList());
            }
        }

        #endregion

        #region Verificaciones de existencia

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Any(u => u.Email.Value == email));
            }
        }

        public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Any(u => u.PhoneNumber.Value == phoneNumber));
            }
        }

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Any(u => u.Id == id));
            }
        }

        #endregion

        #region Consultas con paginación

        public Task<PagedResult<User>> GetUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                var query = _sharedUsers.AsQueryable();
                query = ApplySorting(query, pagination);
                
                var totalCount = query.Count();
                var users = query.Skip(pagination.Skip).Take(pagination.Take).ToList();
                
                return Task.FromResult(new PagedResult<User>(users, totalCount, pagination));
            }
        }

        public Task<PagedResult<User>> SearchUsersAsync(PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                var query = _sharedUsers.AsQueryable();
                
                // Aplicar filtro de búsqueda
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLowerInvariant();
                    query = query.Where(u =>
                        u.Email.Value.ToLowerInvariant().Contains(lowerSearchTerm) ||
                        u.FullName.FirstName.ToLowerInvariant().Contains(lowerSearchTerm) ||
                        u.FullName.LastName.ToLowerInvariant().Contains(lowerSearchTerm) ||
                        u.PhoneNumber.Value.Contains(searchTerm));
                }
                
                query = ApplySorting(query, pagination);
                
                var totalCount = query.Count();
                var users = query.Skip(pagination.Skip).Take(pagination.Take).ToList();
                
                return Task.FromResult(new PagedResult<User>(users, totalCount, pagination));
            }
        }

        #endregion

        #region Consultas con especificaciones

        public Task<PagedResult<User>> GetUsersAsync(ISpecification<User> specification, PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                var query = _sharedUsers.AsQueryable();
                query = query.Where(specification.ToExpression().Compile()).AsQueryable();
                query = ApplySorting(query, pagination);
                
                var totalCount = query.Count();
                var users = query.Skip(pagination.Skip).Take(pagination.Take).ToList();
                
                return Task.FromResult(new PagedResult<User>(users, totalCount, pagination));
            }
        }

        public Task<List<User>> GetUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                var users = _sharedUsers.Where(specification.IsSatisfiedBy).ToList();
                return Task.FromResult(users);
            }
        }

        public Task<int> CountUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Count(specification.IsSatisfiedBy));
            }
        }

        public Task<bool> AnyUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Any(specification.IsSatisfiedBy));
            }
        }

        #endregion

        #region Métodos simples de conteo

        public Task<int> CountUsersAsync(CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Count);
            }
        }

        public Task<bool> AnyUsersAsync(CancellationToken cancellationToken = default)
        {
            lock (_sharedLock)
            {
                return Task.FromResult(_sharedUsers.Any());
            }
        }

        #endregion

        #region Utilidades para tests

        /// <summary>
        /// Sincroniza un usuario desde el repositorio de escritura (llamado por el mock de eventos)
        /// </summary>
        public static void SynchronizeUser(User user)
        {
            lock (_sharedLock)
            {
                if (!_sharedUsers.Any(u => u.Id == user.Id))
                {
                    _sharedUsers.Add(user);
                }
            }
        }

        /// <summary>
        /// Limpia todos los datos compartidos (para aislamiento de tests)
        /// </summary>
        public static void ClearAll()
        {
            lock (_sharedLock)
            {
                _sharedUsers.Clear();
            }
        }

        #endregion

        #region Métodos privados de ayuda

        private static IQueryable<User> ApplySorting(IQueryable<User> query, PaginationParams pagination)
        {
            if (!pagination.HasSorting)
                return query.OrderBy(u => u.Id);

            return pagination.SortBy?.ToLowerInvariant() switch
            {
                "email" => pagination.IsAscending 
                    ? query.OrderBy(u => u.Email.Value)
                    : query.OrderByDescending(u => u.Email.Value),
                "firstname" => pagination.IsAscending
                    ? query.OrderBy(u => u.FullName.FirstName)
                    : query.OrderByDescending(u => u.FullName.FirstName),
                "lastname" => pagination.IsAscending
                    ? query.OrderBy(u => u.FullName.LastName)
                    : query.OrderByDescending(u => u.FullName.LastName),
                "createdat" => pagination.IsAscending
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderBy(u => u.Id)
            };
        }

        #endregion
    }
}
