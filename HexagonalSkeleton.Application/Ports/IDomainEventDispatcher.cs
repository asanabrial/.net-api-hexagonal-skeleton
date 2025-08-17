using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Application.Ports
{
    /// <summary>
    /// Pure domain event dispatcher - no external dependencies
    /// </summary>
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
        Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
