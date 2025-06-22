using HexagonalSkeleton.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for DbContext to handle domain events automatically
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Save changes and automatically publish domain events from all aggregates
        /// </summary>
        public static async Task<int> SaveChangesAndPublishEventsAsync(
            this DbContext context, 
            IMediator mediator, 
            CancellationToken cancellationToken = default)
        {
            // Get all aggregates with domain events before saving
            var aggregatesWithEvents = context.ChangeTracker.Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            // Save changes to database first
            var result = await context.SaveChangesAsync(cancellationToken);

            // Publish domain events after successful save
            await PublishDomainEventsAsync(aggregatesWithEvents, mediator, cancellationToken);

            return result;
        }

        /// <summary>
        /// Publish all domain events from the given aggregates
        /// </summary>
        private static async Task PublishDomainEventsAsync(
            IEnumerable<AggregateRoot> aggregates, 
            IMediator mediator, 
            CancellationToken cancellationToken)
        {
            foreach (var aggregate in aggregates)
            {
                var domainEvents = aggregate.DomainEvents.ToList();
                
                foreach (var domainEvent in domainEvents)
                {
                    await mediator.Publish(domainEvent, cancellationToken);
                }
                
                aggregate.ClearDomainEvents();
            }
        }
    }
}
