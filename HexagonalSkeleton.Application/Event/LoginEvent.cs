using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Application.Event
{
    public class LoginEvent(int userId) : DomainEvent
    {
        public int UserId { get; } = userId;
    }
}
