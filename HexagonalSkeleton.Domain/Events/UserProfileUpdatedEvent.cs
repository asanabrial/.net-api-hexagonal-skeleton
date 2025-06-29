using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Domain event fired when user profile is updated
    /// </summary>
    public class UserProfileUpdatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string? PreviousName { get; }
        public string? NewName { get; }

        public UserProfileUpdatedEvent(Guid userId, string email, string? previousName, string? newName)
        {
            UserId = userId;
            Email = email;
            PreviousName = previousName;
            NewName = newName;
        }
    }
}
