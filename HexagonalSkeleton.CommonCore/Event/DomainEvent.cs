using MediatR;

namespace HexagonalSkeleton.CommonCore.Event
{
    /// <summary>
    /// Base event
    /// </summary>
    public abstract class DomainEvent : INotification
    {
        public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
    }
}
