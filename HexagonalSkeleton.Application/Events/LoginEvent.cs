using MediatR;

namespace HexagonalSkeleton.Application.Events
{
    /// <summary>
    /// Application event for immediate notification when user needs login processing
    /// This is a CQRS/MediatR event for application coordination, not a domain event
    /// </summary>
    public class LoginEvent(Guid userId) : INotification
    {
        public Guid UserId { get; } = userId;
    }
}
