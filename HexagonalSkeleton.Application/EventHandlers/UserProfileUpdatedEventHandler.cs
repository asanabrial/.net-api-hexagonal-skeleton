using HexagonalSkeleton.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.EventHandlers
{
    /// <summary>
    /// Domain event handler for when a user profile is updated
    /// </summary>
    public class UserProfileUpdatedEventHandler : INotificationHandler<UserProfileUpdatedEvent>
    {
        private readonly ILogger<UserProfileUpdatedEventHandler> _logger;

        public UserProfileUpdatedEventHandler(ILogger<UserProfileUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(UserProfileUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "User profile updated - ID: {UserId}, Email: {Email}, Previous Name: {PreviousName}, New Name: {NewName}", 
                notification.UserId, 
                notification.Email, 
                notification.PreviousName, 
                notification.NewName);

            // Here you could:
            // - Update search indexes
            // - Notify connected users
            // - Update external systems
            // - Trigger profile completion checks
            
            await Task.CompletedTask;
        }
    }
}
