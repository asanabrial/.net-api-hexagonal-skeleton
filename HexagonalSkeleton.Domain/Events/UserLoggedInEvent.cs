using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Domain.Events
{    /// <summary>
    /// Domain event raised when a user successfully logs in
    /// This is raised by the User aggregate when RecordLogin() is called
    /// 
    /// This event represents the business fact that a login occurred and should be used for:
    /// - Business-related side effects (analytics, security monitoring)
    /// - Domain-driven concerns (updating user activity, triggering business rules)
    /// 
    /// Note: This is different from LoginEvent (application event) which is used for 
    /// immediate application coordination
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
