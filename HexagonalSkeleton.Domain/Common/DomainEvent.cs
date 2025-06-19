using MediatR;

namespace HexagonalSkeleton.Domain.Common
{
    /// <summary>
    /// Base class for all domain events.
    /// Domain events represent something important that happened in the domain.
    /// </summary>
    public abstract class DomainEvent : INotification
    {
        /// <summary>
        /// When the domain event occurred.
        /// </summary>
        public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;
    }
}
