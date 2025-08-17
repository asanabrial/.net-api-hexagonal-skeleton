using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Test.Unit.Domain.Common
{
    /// <summary>
    /// Test implementation of AggregateRoot for testing purposes
    /// </summary>
    public class TestAggregate : AggregateRoot
    {
        public TestAggregate()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void AddTestEvent(DomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }

        public void RemoveTestEvent(DomainEvent domainEvent)
        {
            RemoveDomainEvent(domainEvent);
        }

        public void SetUpdated()
        {
            MarkAsUpdated();
        }
    }
}
