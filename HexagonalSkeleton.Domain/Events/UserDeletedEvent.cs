using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Domain event raised when a user is deleted (soft delete)
    /// Follows DDD principles - the domain is responsible for raising events
    /// </summary>
    public class UserDeletedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime DeletedAt { get; }

        public UserDeletedEvent(Guid userId, string email, DateTime deletedAt)
        {
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            DeletedAt = deletedAt;
        }
    }
}
