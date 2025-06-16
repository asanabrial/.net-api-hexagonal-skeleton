using HexagonalSkeleton.Domain.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.Event
{
    public class LoginEventHandler(
        ILogger<LoginEventHandler> logger,
        IUserWriteRepository userWriteRepository)
        : INotificationHandler<LoginEvent>
    {
        public async Task Handle(LoginEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("New notification: User logged in. UserId: {User}", notification.UserId);

            await userWriteRepository.SetLastLoginAsync(notification.UserId, cancellationToken);
        }
    }
}
