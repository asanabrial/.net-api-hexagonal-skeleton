using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Application.Ports
{
    /// <summary>
    /// Pure domain event handler interface - no external dependencies
    /// This belongs in Application layer as it's a port (interface)
    /// </summary>
    /// <typeparam name="TDomainEvent">Type of domain event to handle</typeparam>
    public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : DomainEvent
    {
        Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Pure domain event dispatcher - no external dependencies
    /// </summary>
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
        Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
