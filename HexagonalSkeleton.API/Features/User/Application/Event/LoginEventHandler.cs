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
            logger.LogInformation("New notification: User logged in. UserId: {User}", notification.UserId);

            await unitOfWork.Users.SetLastLogin(notification.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
