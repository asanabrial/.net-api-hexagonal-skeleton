using HexagonalSkeleton.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.Event
{
    public class LoginEventHandler(
        ILogger<LoginEventHandler> logger,
        IUserRepository unitOfWork)
        : INotificationHandler<LoginEvent>
    {
        public async Task Handle(LoginEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("New notification: User logged in. UserId: {User}", notification.UserId);

            await unitOfWork.SetLastLogin(notification.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
