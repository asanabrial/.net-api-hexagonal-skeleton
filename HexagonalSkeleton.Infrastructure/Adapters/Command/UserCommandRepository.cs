using AutoMapper;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Infrastructure.Adapters.Command
{
    /// <summary>
    /// Command Repository for User entity write operations following CQRS pattern.
    /// Implements the outbound port for data persistence using PostgreSQL.
    /// CDC is handled automatically by Debezium.
    /// </summary>
    public class UserCommandRepository : IUserWriteRepository
    {
        private readonly CommandDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCommandRepository> _logger;

        public UserCommandRepository(
            CommandDbContext dbContext, 
            IMapper mapper, 
            ILogger<UserCommandRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating user with email: {Email}", user.Email.Value);

            try
            {
                var userEntity = _mapper.Map<UserCommandEntity>(user);
                _dbContext.Users.Add(userEntity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return userEntity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user with email: {Email}", user.Email.Value);
                throw;
            }
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", user.Id);

            try
            {
                var existingEntity = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == user.Id && !u.IsDeleted, cancellationToken);

                if (existingEntity == null)
                {
                    throw new InvalidOperationException($"User with ID {user.Id} not found or deleted");
                }

                // Map domain object to entity
                _mapper.Map(user, existingEntity);
                existingEntity.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);
                user.ClearDomainEvents();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with ID: {UserId}", user.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);

            try
            {
                var userEntity = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

                if (userEntity == null)
                {
                    throw new InvalidOperationException($"User with ID {id} not found or already deleted");
                }

                // Logical deletion (soft delete)
                userEntity.IsDeleted = true;
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user with ID: {UserId}", id);
                throw;
            }
        }

        public async Task SetLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Setting last login for user ID: {UserId}", userId);

            try
            {
                var userEntity = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (userEntity == null)
                {
                    throw new InvalidOperationException($"User with ID {userId} not found or deleted");
                }

                userEntity.LastLogin = DateTime.UtcNow;
                userEntity.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set last login for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting tracked user by ID: {UserId}", id);

            try
            {
                var userEntity = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

                if (userEntity == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found", id);
                    return null;
                }

                var user = _mapper.Map<User>(userEntity);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tracked user by ID: {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetByIdUnfilteredAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting unfiltered user by ID: {UserId}", id);

            try
            {
                var userEntity = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken); // No IsDeleted filter

                if (userEntity == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found", id);
                    return null;
                }

                var user = _mapper.Map<User>(userEntity);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unfiltered user by ID: {UserId}", id);
                throw;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save changes to database");
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user by email for authentication: {Email}", email);

            try
            {
                var userEntity = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

                if (userEntity == null)
                {
                    _logger.LogInformation("User with email {Email} not found", email);
                    return null;
                }

                var user = _mapper.Map<User>(userEntity);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by email: {Email}", email);
                throw;
            }
        }
    }
}
