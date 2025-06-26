using HexagonalSkeleton.Domain.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.Events
{
    /// <summary>
    /// Application event handler for immediate login processing coordination
    /// This handles the LoginEvent (application event) for coordinating login-related tasks
    /// Note: This is different from UserLoggedInEvent (domain event) which is raised by the User aggregate
    /// </summary>
    public class LoginEventHandler(
        ILogger<LoginEventHandler> logger,
        IUserWriteRepository userWriteRepository)
        : INotificationHandler<LoginEvent>
    {
        public async Task Handle(LoginEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing login event for user {UserId}", notification.UserId);

            // Update last login timestamp and raise domain events
            // This will internally call user.RecordLogin() which raises UserLoggedInEvent
            await userWriteRepository.SetLastLoginAsync(notification.UserId, cancellationToken);
            
            logger.LogInformation("Login event processed successfully for user {UserId}", notification.UserId);
        }
    }
}
