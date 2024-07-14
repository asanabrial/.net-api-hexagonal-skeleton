using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Event
{
    public class LoginEventHandler(
        ILogger<LoginEventHandler> logger,
        IUnitOfWork unitOfWork)
        : INotificationHandler<LoginEvent>
    {
        public async Task Handle(LoginEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("New notification: User logged in. {User}", notification.User);
            await unitOfWork.Users.SetLastLogin(notification.User.Email, cancellationToken);
        }
    }
}
