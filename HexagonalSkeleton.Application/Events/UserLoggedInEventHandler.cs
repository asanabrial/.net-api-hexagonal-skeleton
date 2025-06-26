using HexagonalSkeleton.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.Events
{
    /// <summary>
    /// Domain event handler for when a user logs in
    /// </summary>
    public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
    {
        private readonly ILogger<UserLoggedInEventHandler> _logger;

        public UserLoggedInEventHandler(ILogger<UserLoggedInEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "User logged in - ID: {UserId}, Email: {Email}, Login Time: {LoginTime}", 
                notification.UserId, 
                notification.Email, 
                notification.LoginTime);

            // Here you could:
            // - Update user activity tracking
            // - Check for security alerts
            // - Update online status
            // - Trigger location-based features
            
            await Task.CompletedTask;
        }
    }
}
