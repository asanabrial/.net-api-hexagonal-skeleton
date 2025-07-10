using HexagonalSkeleton.Application.Ports;
using HexagonalSkeleton.Domain.Common;
using HexagonalSkeleton.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Infrastructure.Services
{
    /// <summary>
    /// Pure domain event dispatcher implementation
    /// Uses service provider to find and execute handlers without MediatR dependency
    /// </summary>
    public class PureDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PureDomainEventDispatcher> _logger;

        public PureDomainEventDispatcher(IServiceProvider serviceProvider, ILogger<PureDomainEventDispatcher> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent == null)
            {
                _logger.LogWarning("Attempted to dispatch null domain event");
                return;
            }

            _logger.LogDebug("Dispatching domain event {EventType} with ID {EventId}", 
                domainEvent.GetType().Name, domainEvent.EventId);

            // Get all handlers for this specific event type
            var handlers = GetHandlersForEvent(domainEvent);
            
            if (!handlers.Any())
            {
                _logger.LogDebug("No handlers found for domain event {EventType}", domainEvent.GetType().Name);
                return;
            }

            // Execute all handlers
            var tasks = handlers.Select(handler => ExecuteHandler(handler, domainEvent, cancellationToken));
            await Task.WhenAll(tasks);
        }

        public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            if (domainEvents == null || !domainEvents.Any())
            {
                return;
            }

            var tasks = domainEvents.Select(evt => DispatchAsync(evt, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private IEnumerable<object> GetHandlersForEvent(DomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            
            return _serviceProvider.GetServices(handlerType);
        }

        private async Task ExecuteHandler(object handler, DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                // Use reflection to call HandleAsync method
                var method = handler.GetType().GetMethod("HandleAsync");
                if (method != null)
                {
                    var task = (Task)method.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                    await task;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing domain event handler {HandlerType} for event {EventType}", 
                    handler.GetType().Name, domainEvent.GetType().Name);
                // Don't rethrow - we don't want one handler failure to stop others
            }
        }
    }
}
