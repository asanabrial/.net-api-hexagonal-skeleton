using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    /// <summary>
    /// Adapter for user write operations
    /// Implements the outbound port for data persistence
    /// Ensures domain events are published after successful persistence
    /// </summary>
    public class UserWriteRepositoryAdapter : IUserWriteRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UserWriteRepositoryAdapter(AppDbContext dbContext, IMapper mapper, IMediator mediator)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _mediator = mediator;
        }        public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<UserEntity>(user);
            _dbContext.Users.Add(entity);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // The ID is handled by the mapping profile, so no need to set it manually
            
            // Publish domain events after successful save
            await PublishDomainEventsAsync(user, cancellationToken);
            
            return entity.Id;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<UserEntity>(user);
            _dbContext.Users.Update(entity);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Publish domain events after successful save
            await PublishDomainEventsAsync(user, cancellationToken);
        }        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(id);
            if (entity != null)
            {
                _dbContext.Users.Remove(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetLastLoginAsync(int userId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(userId);
            if (entity != null)
            {
                entity.LastLogin = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
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

        /// <summary>
        /// Publish domain events from the aggregate
        /// </summary>
        private async Task PublishDomainEventsAsync(User user, CancellationToken cancellationToken)
        {
            var domainEvents = user.DomainEvents.ToList();
            
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            
            user.ClearDomainEvents();
        }
    }
}
