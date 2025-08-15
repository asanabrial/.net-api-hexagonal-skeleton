using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test.TestInfrastructure.Helpers
{
    /// <summary>
    /// Helper for tests that need synchronization with CDC events.
    /// Provides convenient methods to wait for CDC operations to complete.
    /// </summary>
    public class CdcTestHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CdcTestHelper> _logger;

        public CdcTestHelper(IServiceProvider serviceProvider, ILogger<CdcTestHelper> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Waits for a created user to be synchronized by CDC
        /// </summary>
        /// <param name="userId">ID of the created user</param>
        /// <param name="timeout">Timeout in seconds (default: 10)</param>
        /// <returns>True if synchronization was successful</returns>
        public async Task<bool> WaitForUserCreatedAsync(Guid userId, int timeoutSeconds = 10)
        {
            var syncHelper = _serviceProvider.GetRequiredService<TestInfrastructure.Services.CdcSynchronizationHelper>();
            var success = await syncHelper.WaitForUserCreatedAsync(userId, TimeSpan.FromSeconds(timeoutSeconds));
            
            if (success)
            {
                _logger.LogDebug("CDC synchronization completed for user creation: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("CDC synchronization timeout for user creation: {UserId} after {Timeout}s", 
                    userId, timeoutSeconds);
            }
            
            return success;
        }

        /// <summary>
        /// Waits for an updated user to be synchronized by CDC
        /// </summary>
        /// <param name="userId">ID of the updated user</param>
        /// <param name="timeout">Timeout in seconds (default: 10)</param>
        /// <returns>True if synchronization was successful</returns>
        public async Task<bool> WaitForUserUpdatedAsync(Guid userId, int timeoutSeconds = 10)
        {
            var syncHelper = _serviceProvider.GetRequiredService<TestInfrastructure.Services.CdcSynchronizationHelper>();
            var success = await syncHelper.WaitForUserUpdatedAsync(userId, TimeSpan.FromSeconds(timeoutSeconds));
            
            if (success)
            {
                _logger.LogDebug("CDC synchronization completed for user update: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("CDC synchronization timeout for user update: {UserId} after {Timeout}s", 
                    userId, timeoutSeconds);
            }
            
            return success;
        }

        /// <summary>
        /// Waits for a deleted user to be synchronized by CDC
        /// </summary>
        /// <param name="userId">ID of the deleted user</param>
        /// <param name="timeout">Timeout in seconds (default: 10)</param>
        /// <returns>True if synchronization was successful</returns>
        public async Task<bool> WaitForUserDeletedAsync(Guid userId, int timeoutSeconds = 10)
        {
            var syncHelper = _serviceProvider.GetRequiredService<TestInfrastructure.Services.CdcSynchronizationHelper>();
            var success = await syncHelper.WaitForUserDeletedAsync(userId, TimeSpan.FromSeconds(timeoutSeconds));
            
            if (success)
            {
                _logger.LogDebug("CDC synchronization completed for user deletion: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("CDC synchronization timeout for user deletion: {UserId} after {Timeout}s", 
                    userId, timeoutSeconds);
            }
            
            return success;
        }

        /// <summary>
        /// Helper that combines user creation and CDC synchronization wait
        /// </summary>
        /// <param name="createUserAction">Action that creates the user and returns its ID</param>
        /// <param name="timeout">Timeout en segundos (default: 10)</param>
        /// <returns>Tuple with the UserId and if the synchronization was successful</returns>
        public async Task<(Guid userId, bool syncSuccess)> CreateUserAndWaitForSyncAsync(
            Func<Task<Guid>> createUserAction, 
            int timeoutSeconds = 10)
        {
            if (createUserAction == null)
                throw new ArgumentNullException(nameof(createUserAction));

            _logger.LogDebug("Creating user and waiting for CDC synchronization...");

            // Create the user
            var userId = await createUserAction();
            
            // Wait for CDC synchronization
            var syncSuccess = await WaitForUserCreatedAsync(userId, timeoutSeconds);
            
            return (userId, syncSuccess);
        }

        /// <summary>
        /// Helper that combines user update and CDC synchronization wait
        /// </summary>
        /// <param name="userId">ID of the user to update</param>
        /// <param name="updateUserAction">Action that updates the user</param>
        /// <param name="timeout">Timeout in seconds (default: 10)</param>
        /// <returns>True if the update and synchronization were successful</returns>
        public async Task<bool> UpdateUserAndWaitForSyncAsync(
            Guid userId,
            Func<Task> updateUserAction, 
            int timeoutSeconds = 10)
        {
            if (updateUserAction == null)
                throw new ArgumentNullException(nameof(updateUserAction));

            _logger.LogDebug("Updating user {UserId} and waiting for CDC synchronization...", userId);

            // Update the user
            await updateUserAction();
            
            // Wait for CDC synchronization
            return await WaitForUserUpdatedAsync(userId, timeoutSeconds);
        }

        /// <summary>
        /// Helper that combines user deletion and CDC synchronization wait
        /// </summary>
        /// <param name="userId">ID of the user to delete</param>
        /// <param name="deleteUserAction">Action that deletes the user</param>
        /// <param name="timeout">Timeout in seconds (default: 10)</param>
        /// <returns>True if the deletion and synchronization were successful</returns>
        public async Task<bool> DeleteUserAndWaitForSyncAsync(
            Guid userId,
            Func<Task> deleteUserAction, 
            int timeoutSeconds = 10)
        {
            if (deleteUserAction == null)
                throw new ArgumentNullException(nameof(deleteUserAction));

            _logger.LogDebug("Deleting user {UserId} and waiting for CDC synchronization...", userId);

            // Delete the user
            await deleteUserAction();
            
            // Wait for CDC synchronization
            return await WaitForUserDeletedAsync(userId, timeoutSeconds);
        }
    }
}
