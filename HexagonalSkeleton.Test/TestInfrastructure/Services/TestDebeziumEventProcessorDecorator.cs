using HexagonalSkeleton.Infrastructure.CDC;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test.TestInfrastructure.Services
{
    /// <summary>
    /// Decorator of the DebeziumEventProcessor that adds synchronization capabilities for tests
    /// without modifying production logic. Allows waiting for CDC events to be processed.
    /// </summary>
    public class TestDebeziumEventProcessorDecorator
    {
        private readonly DebeziumEventProcessor _innerProcessor;
        private readonly CdcSynchronizationHelper _syncHelper;
        private readonly ILogger<TestDebeziumEventProcessorDecorator> _logger;

        public TestDebeziumEventProcessorDecorator(
            DebeziumEventProcessor innerProcessor,
            CdcSynchronizationHelper syncHelper,
            ILogger<TestDebeziumEventProcessorDecorator> logger)
        {
            _innerProcessor = innerProcessor ?? throw new ArgumentNullException(nameof(innerProcessor));
            _syncHelper = syncHelper ?? throw new ArgumentNullException(nameof(syncHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes a CDC event and notifies the synchronization helper when it completes
        /// </summary>
        public async Task<bool> ProcessChangeEventAsync(string eventPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                // Process the event using the original processor
                var result = await _innerProcessor.ProcessChangeEventAsync(eventPayload, cancellationToken);

                // If successful, notify the synchronization helper
                if (result)
                {
                    NotifyEventProcessed(eventPayload);
                }
                else
                {
                    NotifyEventFailed(eventPayload);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error in TestDebeziumEventProcessorDecorator");
                NotifyEventFailed(eventPayload);
                throw;
            }
        }

        /// <summary>
        /// Notifica al helper que un evento fue procesado exitosamente
        /// </summary>
        private void NotifyEventProcessed(string eventPayload)
        {
            try
            {
                var (userId, operation) = ExtractEventInfo(eventPayload);
                if (userId.HasValue)
                {
                    switch (operation?.ToLower())
                    {
                        case "c":
                        case "r": // create or read (snapshot)
                            _syncHelper.NotifyUserCreated(userId.Value);
                            _logger.LogDebug("🔔 Notified CDC sync helper: User {UserId} created", userId.Value);
                            break;
                        case "u": // update
                            _syncHelper.NotifyUserUpdated(userId.Value);
                            _logger.LogDebug("🔔 Notified CDC sync helper: User {UserId} updated", userId.Value);
                            break;
                        case "d": // delete
                            _syncHelper.NotifyUserDeleted(userId.Value);
                            _logger.LogDebug("🔔 Notified CDC sync helper: User {UserId} deleted", userId.Value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify CDC sync helper for successful event");
            }
        }

        /// <summary>
        /// Notifies the helper that an event failed in processing
        /// </summary>
        private void NotifyEventFailed(string eventPayload)
        {
            try
            {
                var (userId, operation) = ExtractEventInfo(eventPayload);
                if (userId.HasValue && !string.IsNullOrEmpty(operation))
                {
                    _syncHelper.NotifyOperationFailed(userId.Value, operation);
                    _logger.LogDebug("Notified CDC sync helper: User {UserId} operation {Operation} failed", 
                        userId.Value, operation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify CDC sync helper for failed event");
            }
        }

        /// <summary>
        /// Extracts information from the CDC event for synchronization
        /// </summary>
        private (Guid? userId, string? operation) ExtractEventInfo(string eventPayload)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(eventPayload);
                var root = doc.RootElement;

                // Obtener operation
                var operation = root.TryGetProperty("op", out var opElement) ? opElement.GetString() : null;

                // Obtener userId del payload "after" o "before"
                Guid? userId = null;
                if (root.TryGetProperty("after", out var afterElement) && afterElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    if (afterElement.TryGetProperty("Id", out var idElement))
                    {
                        if (Guid.TryParse(idElement.GetString(), out var parsedId))
                            userId = parsedId;
                    }
                }
                else if (root.TryGetProperty("before", out var beforeElement) && beforeElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    if (beforeElement.TryGetProperty("Id", out var idElement))
                    {
                        if (Guid.TryParse(idElement.GetString(), out var parsedId))
                            userId = parsedId;
                    }
                }

                return (userId, operation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract event info from CDC payload");
                return (null, null);
            }
        }
    }
}
