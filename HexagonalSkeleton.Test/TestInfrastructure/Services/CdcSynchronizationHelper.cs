using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test.TestInfrastructure.Services
{
    /// <summary>
    /// Helper for synchronizing tests with CDC events without using hardcoded delays.
    /// Provides methods to wait for specific events to be processed by the CDC pipeline.
    /// </summary>
    public class CdcSynchronizationHelper : IAsyncDisposable
    {
        private readonly ILogger<CdcSynchronizationHelper> _logger;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingOperations = new();
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public CdcSynchronizationHelper(ILogger<CdcSynchronizationHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Waits for a user creation event to be processed by CDC
        /// </summary>
        /// <param name="userId">ID of the created user</param>
        /// <param name="timeout">Timeout opcional (default: 10 segundos)</param>
        /// <returns>True si el evento fue procesado exitosamente</returns>
        public async Task<bool> WaitForUserCreatedAsync(Guid userId, TimeSpan? timeout = null)
        {
            var key = $"user_created_{userId}";
            return await WaitForOperationAsync(key, timeout ?? _defaultTimeout);
        }

        /// <summary>
        /// Waits for a user update event to be processed by CDC
        /// </summary>
        /// <param name="userId">ID of the updated user</param>
        /// <param name="timeout">Timeout opcional (default: 10 segundos)</param>
        /// <returns>True si el evento fue procesado exitosamente</returns>
        public async Task<bool> WaitForUserUpdatedAsync(Guid userId, TimeSpan? timeout = null)
        {
            var key = $"user_updated_{userId}";
            return await WaitForOperationAsync(key, timeout ?? _defaultTimeout);
        }

        /// <summary>
        /// Waits for a user deletion event to be processed by CDC
        /// </summary>
        /// <param name="userId">ID of the deleted user</param>
        /// <param name="timeout">Timeout opcional (default: 10 segundos)</param>
        /// <returns>True si el evento fue procesado exitosamente</returns>
        public async Task<bool> WaitForUserDeletedAsync(Guid userId, TimeSpan? timeout = null)
        {
            var key = $"user_deleted_{userId}";
            return await WaitForOperationAsync(key, timeout ?? _defaultTimeout);
        }

        /// <summary>
        /// Notifies that a user creation event has been processed
        /// This method should be called from the DebeziumEventProcessor
        /// </summary>
        /// <param name="userId">ID of the created user</param>
        public void NotifyUserCreated(Guid userId)
        {
            var key = $"user_created_{userId}";
            NotifyOperationCompleted(key, true);
        }

        /// <summary>
        /// Notifies that a user update event has been processed
        /// This method should be called from the DebeziumEventProcessor
        /// </summary>
        /// <param name="userId">ID of the updated user</param>
        public void NotifyUserUpdated(Guid userId)
        {
            var key = $"user_updated_{userId}";
            NotifyOperationCompleted(key, true);
        }

        /// <summary>
        /// Notifies that a user deletion event has been processed
        /// This method should be called from the DebeziumEventProcessor
        /// </summary>
        /// <param name="userId">ID of the deleted user</param>
        public void NotifyUserDeleted(Guid userId)
        {
            var key = $"user_deleted_{userId}";
            NotifyOperationCompleted(key, true);
        }

        /// <summary>
        /// Notifica que un evento ha fallado en el procesamiento
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="operation">Type of operation (created, updated, deleted)</param>
        public void NotifyOperationFailed(Guid userId, string operation)
        {
            var key = $"user_{operation}_{userId}";
            NotifyOperationCompleted(key, false);
        }

        private async Task<bool> WaitForOperationAsync(string operationKey, TimeSpan timeout)
        {
            var tcs = _pendingOperations.GetOrAdd(operationKey, _ => new TaskCompletionSource<bool>());
            
            _logger.LogDebug("Waiting for CDC operation: {OperationKey} (timeout: {Timeout}s)", 
                operationKey, timeout.TotalSeconds);

            try
            {
                using var cts = new CancellationTokenSource(timeout);
                var timeoutTask = Task.Delay(timeout, cts.Token);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logger.LogWarning("Timeout waiting for CDC operation: {OperationKey} after {Timeout}s", 
                        operationKey, timeout.TotalSeconds);
                    _pendingOperations.TryRemove(operationKey, out _);
                    return false;
                }

                var result = await tcs.Task;
                _logger.LogDebug("CDC operation completed: {OperationKey} -> {Result}", operationKey, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for CDC operation: {OperationKey}", operationKey);
                _pendingOperations.TryRemove(operationKey, out _);
                return false;
            }
        }

        private void NotifyOperationCompleted(string operationKey, bool success)
        {
            if (_pendingOperations.TryRemove(operationKey, out var tcs))
            {
                _logger.LogDebug("🔔 Notifying CDC operation completion: {OperationKey} -> {Success}", 
                    operationKey, success);
                tcs.SetResult(success);
            }
        }

        /// <summary>
        /// Limpia todas las operaciones pendientes
        /// </summary>
        public void ClearPendingOperations()
        {
            foreach (var kvp in _pendingOperations)
            {
                kvp.Value.TrySetCanceled();
            }
            _pendingOperations.Clear();
            _logger.LogDebug("🧹 Cleared all pending CDC operations");
        }

        public ValueTask DisposeAsync()
        {
            ClearPendingOperations();
            _logger.LogDebug("CdcSynchronizationHelper disposed");
            return ValueTask.CompletedTask;
        }
    }
}
