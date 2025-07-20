using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Domain event fired when a new user is created in the system.
    /// Contains essential information for downstream processes like welcome emails, analytics, etc.
    /// Immutable event following DDD principles - once created, it cannot be modified.
    /// </summary>
    public sealed class UserCreatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string Name { get; }
        public string Surname { get; }
        public DateTime CreatedAt { get; }
        public string PhoneNumber { get; }

        public UserCreatedEvent(Guid userId, string email, string name, string surname, string phoneNumber)
        {
            // Guard clauses for business rule enforcement
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty", nameof(userId));
            
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Surname = surname ?? throw new ArgumentNullException(nameof(surname));
            PhoneNumber = phoneNumber ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
