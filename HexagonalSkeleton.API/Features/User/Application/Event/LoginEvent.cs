using HexagonalSkeleton.API.Features.User.Domain;
using HexagonalSkeleton.CommonCore.Event;

namespace HexagonalSkeleton.API.Features.User.Application.Event
{
    public class LoginEvent(UserEntity user) : DomainEvent
    {
        public UserEntity User { get; } = user;
    }
}
