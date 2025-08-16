using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test.TestInfrastructure.Helpers;

public class CdcTestHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CdcTestHelper> _logger;

    public CdcTestHelper(IServiceProvider serviceProvider, ILogger<CdcTestHelper> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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

    public async Task<(Guid userId, bool syncSuccess)> CreateUserAndWaitForSyncAsync(
        Func<Task<Guid>> createUserAction, 
        int timeoutSeconds = 10)
    {
        if (createUserAction == null)
            throw new ArgumentNullException(nameof(createUserAction));

        _logger.LogDebug("Creating user and waiting for CDC synchronization...");

        var userId = await createUserAction();
        var syncSuccess = await WaitForUserCreatedAsync(userId, timeoutSeconds);
        
        return (userId, syncSuccess);
    }

    public async Task<bool> UpdateUserAndWaitForSyncAsync(
        Guid userId,
        Func<Task> updateUserAction, 
        int timeoutSeconds = 10)
    {
        if (updateUserAction == null)
            throw new ArgumentNullException(nameof(updateUserAction));

        _logger.LogDebug("Updating user {UserId} and waiting for CDC synchronization...", userId);

        await updateUserAction();
        return await WaitForUserUpdatedAsync(userId, timeoutSeconds);
    }

    public async Task<bool> DeleteUserAndWaitForSyncAsync(
        Guid userId,
        Func<Task> deleteUserAction, 
        int timeoutSeconds = 10)
    {
        if (deleteUserAction == null)
            throw new ArgumentNullException(nameof(deleteUserAction));

        _logger.LogDebug("Deleting user {UserId} and waiting for CDC synchronization...", userId);

        await deleteUserAction();
        return await WaitForUserDeletedAsync(userId, timeoutSeconds);
    }
}
