using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Test.TestInfrastructure.Mocks
{
    /// <summary>
    /// Simple in-memory implementation of IUserReadRepository for testing
    /// Avoids QueryDbContext dependency issues in integration tests
    /// </summary>
    public class InMemoryUserReadRepository : IUserReadRepository
    {
        private readonly List<User> _users = new();

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var exists = _users.Any(u => u.Email.Value.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var exists = _users.Any(u => u.PhoneNumber.Value == phoneNumber);
            return Task.FromResult(exists);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = _users.FirstOrDefault(u => u.Email.Value.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var user = _users.FirstOrDefault(u => u.PhoneNumber.Value == phoneNumber);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<User>> GetByLocationAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default)
        {
            // Simple implementation for testing
            var users = _users.Where(u => 
                Math.Abs(u.Location.Latitude - latitude) <= radiusKm &&
                Math.Abs(u.Location.Longitude - longitude) <= radiusKm
            );
            return Task.FromResult(users);
        }

        public Task<IEnumerable<User>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var users = _users
                .Where(u => 
                    u.FullName.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Skip(page * pageSize)
                .Take(pageSize);
            return Task.FromResult(users);
        }

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Count);
        }

        public Task<IEnumerable<User>> GetRecentlyActiveAsync(int count, CancellationToken cancellationToken = default)
        {
            var users = _users
                .OrderByDescending(u => u.LastLogin)
                .Take(count);
            return Task.FromResult(users);
        }

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.ToList());
        }

        public Task<PagedResult<User>> GetUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            var query = _users.AsQueryable();

            // Apply sorting if specified
            if (pagination.HasSorting)
            {
                query = pagination.SortBy?.ToLowerInvariant() switch
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
                    _ => query.OrderBy(u => u.Id) // Default sorting
                };
            }
            else
            {
                query = query.OrderBy(u => u.Id); // Default sorting
            }

            var totalCount = query.Count();
            var users = query.Skip(pagination.Skip).Take(pagination.Take).ToList();

            var result = new PagedResult<User>(users, totalCount, pagination);
            return Task.FromResult(result);
        }

        public Task<PagedResult<User>> SearchUsersAsync(PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLowerInvariant();
                query = query.Where(u =>
                    u.Email.Value.ToLowerInvariant().Contains(lowerSearchTerm) ||
                    u.FullName.FirstName.ToLowerInvariant().Contains(lowerSearchTerm) ||
                    u.FullName.LastName.ToLowerInvariant().Contains(lowerSearchTerm));
            }

            // Apply sorting if specified
            if (pagination.HasSorting)
            {
                query = pagination.SortBy?.ToLowerInvariant() switch
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
                    _ => query.OrderBy(u => u.Id) // Default sorting
                };
            }
            else
            {
                query = query.OrderBy(u => u.Id); // Default sorting
            }

            var totalCount = query.Count();
            var users = query.Skip(pagination.Skip).Take(pagination.Take).ToList();

            var result = new PagedResult<User>(users, totalCount, pagination);
            return Task.FromResult(result);
        }

        public Task<PagedResult<User>> GetUsersAsync(ISpecification<User> specification, PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            var query = _users.AsQueryable();

            // Apply specification filter
            query = query.Where(specification.ToExpression().Compile()).AsQueryable();

            // Apply sorting if specified
            if (pagination.HasSorting)
            {
                query = pagination.SortBy?.ToLowerInvariant() switch
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
                    _ => query.OrderBy(u => u.Id) // Default sorting
                };
            }
            else
            {
                query = query.OrderBy(u => u.Id); // Default sorting
            }

            var totalCount = query.Count();
            var users = query.Skip(pagination.Skip).Take(pagination.Take).ToList();

            var result = new PagedResult<User>(users, totalCount, pagination);
            return Task.FromResult(result);
        }

        public Task<List<User>> GetUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            var query = _users.AsQueryable();
            var users = query.Where(specification.ToExpression().Compile()).ToList();
            return Task.FromResult(users);
        }

        public Task<int> CountUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            var query = _users.AsQueryable();
            var count = query.Where(specification.ToExpression().Compile()).Count();
            return Task.FromResult(count);
        }

        public Task<bool> AnyUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            var query = _users.AsQueryable();
            var exists = query.Where(specification.ToExpression().Compile()).Any();
            return Task.FromResult(exists);
        }

        // Helper methods for testing
        public void AddUser(User user)
        {
            _users.Add(user);
        }

        public void Clear()
        {
            _users.Clear();
        }
    }
}
