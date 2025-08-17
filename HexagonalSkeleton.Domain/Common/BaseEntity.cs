namespace HexagonalSkeleton.Domain.Common
{
    /// <summary>
    /// Base entity for all domain entities
    /// Provides common audit properties following DDD principles
    /// Inherits from AggregateRoot to support domain events
    /// </summary>
    public abstract class BaseEntity : AggregateRoot
    {
        // All functionality is inherited from AggregateRoot
        // This class exists for future extension and semantic clarity
    }
}
