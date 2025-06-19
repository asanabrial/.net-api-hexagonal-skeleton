using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Common
{
    /// <summary>
    /// Base class for aggregate roots in DDD
    /// </summary>
    public abstract class AggregateRoot
    {
        private readonly List<DomainEvent> _domainEvents = new();

        public int Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public bool IsDeleted { get; protected set; }

        /// <summary>
        /// Get domain events for this aggregate
        /// </summary>
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Add domain event to be published
        /// </summary>
        protected void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Remove domain event
        /// </summary>
        protected void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        /// <summary>
        /// Clear all domain events
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        /// <summary>
        /// Mark entity as deleted (soft delete)
        /// </summary>
        public virtual void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restore deleted entity
        /// </summary>
        public virtual void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark entity as updated
        /// </summary>
        protected void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
