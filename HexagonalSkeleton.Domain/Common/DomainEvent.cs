namespace HexagonalSkeleton.Domain.Common
{
    /// <summary>
    /// Base class for all domain events.
    /// Domain events represent something important that happened in the domain.
    /// This is a pure domain concept without external dependencies.
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// Unique identifier for this domain event instance.
        /// </summary>
        public Guid EventId { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// When the domain event occurred.
        /// </summary>
        public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Version of the event for potential schema evolution.
        /// </summary>
        public int Version { get; protected set; } = 1;
    }
}
