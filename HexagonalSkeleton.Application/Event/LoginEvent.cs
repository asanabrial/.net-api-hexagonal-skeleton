using MediatR;

namespace HexagonalSkeleton.Application.Event
{
    /// <summary>
    /// Application event for immediate notification when user needs login processing
    /// This is a CQRS/MediatR event for application coordination, not a domain event
    /// </summary>
    public class LoginEvent(int userId) : INotification
    {
        public int UserId { get; } = userId;
    }
}
