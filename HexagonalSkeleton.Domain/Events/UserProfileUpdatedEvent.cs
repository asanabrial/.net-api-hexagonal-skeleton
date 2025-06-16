using HexagonalSkeleton.CommonCore.Event;

namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Domain event fired when user profile is updated
    /// </summary>
    public class UserProfileUpdatedEvent : DomainEvent
    {
        public int UserId { get; }
        public string Email { get; }
        public string? PreviousName { get; }
        public string? NewName { get; }

        public UserProfileUpdatedEvent(int userId, string email, string? previousName, string? newName)
        {
            UserId = userId;
            Email = email;
            PreviousName = previousName;
            NewName = newName;
        }
    }
}
