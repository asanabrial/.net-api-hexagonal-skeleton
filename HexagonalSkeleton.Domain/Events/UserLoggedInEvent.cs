using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Domain event fired when user logs in
    /// </summary>
    public class UserLoggedInEvent : DomainEvent
    {
        public int UserId { get; }
        public string Email { get; }
        public DateTime LoginTime { get; }

        public UserLoggedInEvent(int userId, string email, DateTime loginTime)
        {
            UserId = userId;
            Email = email;
            LoginTime = loginTime;
        }
    }
}
