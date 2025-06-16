using HexagonalSkeleton.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.EventHandlers
{
    /// <summary>
    /// Domain event handler for when a user is created
    /// </summary>
    public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "User created - ID: {UserId}, Email: {Email}, Name: {Name} {Surname}", 
                notification.UserId, 
                notification.Email, 
                notification.Name, 
                notification.Surname);

            // Here you could:
            // - Send welcome email
            // - Create default user settings
            // - Initialize user analytics
            // - Publish to external systems
            
            await Task.CompletedTask;
        }
    }
}
