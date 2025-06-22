using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure.Extensions;
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

            // Save changes and publish domain events atomically
            await SaveChangesAndPublishEventsAsync(new[] { user }, cancellationToken);

            return entity.Id;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<UserEntity>(user);
            _dbContext.Users.Update(entity);
            
            // Save changes and publish domain events atomically
            await SaveChangesAndPublishEventsAsync(new[] { user }, cancellationToken);
        }public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(id);
            if (entity != null)
            {
                _dbContext.Users.Remove(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }        public async Task SetLastLoginAsync(int userId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FindAsync(userId);
            if (entity != null)
            {
                // Map to domain aggregate to trigger domain events
                var user = _mapper.Map<User>(entity);
                if (user != null)
                {
                    // Record login in domain - this will raise UserLoggedInEvent
                    user.RecordLogin();
                    
                    // Map back to entity
                    _mapper.Map(user, entity);
                    
                    // Save changes and publish domain events atomically
                    await SaveChangesAndPublishEventsAsync(new[] { user }, cancellationToken);
                }
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
        }        /// <summary>
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

        /// <summary>
        /// Save changes and publish domain events from all aggregates atomically
        /// </summary>
        private async Task SaveChangesAndPublishEventsAsync(IEnumerable<User> aggregates, CancellationToken cancellationToken)
        {
            // Save changes to database first
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Publish domain events after successful save
            foreach (var aggregate in aggregates)
            {
                await PublishDomainEventsAsync(aggregate, cancellationToken);
            }
        }
    }
}
