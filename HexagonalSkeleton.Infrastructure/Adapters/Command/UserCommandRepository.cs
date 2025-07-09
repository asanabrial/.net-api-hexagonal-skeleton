using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;

namespace HexagonalSkeleton.Infrastructure.Adapters.Command
{
    /// <summary>
    /// Command repository adapter for user write operations
    /// Implements the outbound port for data persistence
    /// Follows CQRS pattern - only handles write operations
    /// Uses PostgreSQL for transactional consistency
    /// </summary>
    public class UserCommandRepository : IUserWriteRepository
    {
        private readonly CommandDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UserCommandRepository> _logger;

        public UserCommandRepository(
            CommandDbContext dbContext, 
            IMapper mapper, 
            IMediator mediator,
            ILogger<UserCommandRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
                
                // Publish domain events after successful persistence
                await PublishDomainEventsAsync(user, cancellationToken);
                
                _logger.LogInformation("User created successfully with ID: {UserId}", userEntity.Id);
                return userEntity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", user.Email.Value);
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
                    throw new InvalidOperationException($"User with ID {user.Id} not found or is deleted");
                }

                _mapper.Map(user, existingEntity);
                existingEntity.UpdateTimestamp();

                await _dbContext.SaveChangesAsync(cancellationToken);
                
                // Publish domain events after successful persistence
                await PublishDomainEventsAsync(user, cancellationToken);
                
                _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
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

                userEntity.Delete();
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
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

                if (userEntity != null)
                {
                    userEntity.LastLogin = DateTime.UtcNow;
                    userEntity.UpdateTimestamp();
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                
                _logger.LogInformation("Last login updated for user ID: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting last login for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userEntity = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

            return userEntity != null ? _mapper.Map<User>(userEntity) : null;
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var userEntity = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

            return userEntity != null ? _mapper.Map<User>(userEntity) : null;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task PublishDomainEventsAsync(User user, CancellationToken cancellationToken)
        {
            var domainEvents = user.DomainEvents.ToList();
            user.ClearDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
