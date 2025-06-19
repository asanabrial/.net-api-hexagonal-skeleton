using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Domain event fired when a new user is created
    /// Contains essential information for downstream processes like welcome emails, analytics, etc.
    /// </summary>
    public class UserCreatedEvent : DomainEvent
    {
        public int UserId { get; }
        public string Email { get; }
        public string Name { get; }
        public string Surname { get; }
        public DateTime CreatedAt { get; }
        public string PhoneNumber { get; }

        public UserCreatedEvent(int userId, string email, string name, string surname, string phoneNumber)
        {
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Surname = surname ?? throw new ArgumentNullException(nameof(surname));
            PhoneNumber = phoneNumber ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
