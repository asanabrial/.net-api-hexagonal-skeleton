using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HexagonalSkeleton.Test.TestInfrastructure.Helpers
{
    /// <summary>
    /// Helper for waiting for MongoDB synchronization during CDC testing
    /// Provides polling methods to wait for data changes in the read-side database
    /// </summary>
    public class MongoDbSyncHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(100);
        private readonly ILogger<MongoDbSyncHelper> _logger;

        public MongoDbSyncHelper(IServiceProvider serviceProvider, ILogger<MongoDbSyncHelper> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Waits for a user to appear in MongoDB after creation
        /// </summary>
        /// <param name="userId">ID of the user to wait for</param>
        /// <param name="timeout">Maximum time to wait (default: 30s)</param>
        /// <returns>True if the user appears within the timeout period</returns>
        public async Task<bool> WaitForUserExistsAsync(Guid userId, TimeSpan? timeout = null)
        {
            return await WaitForUserCreationAsync(userId, timeout);
        }

        /// <summary>
        /// Waits for a user to appear in MongoDB after creation
        /// </summary>
        /// <param name="userId">ID of the user to wait for</param>
        /// <param name="timeout">Maximum time to wait (default: 30s)</param>
        /// <returns>True if the user appears within the timeout period</returns>
        public async Task<bool> WaitForUserCreationAsync(Guid userId, TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? _defaultTimeout;
            var cancellationToken = new CancellationTokenSource(actualTimeout).Token;

            _logger.LogDebug("Waiting for user {UserId} to appear in MongoDB (timeout: {Timeout}s)", 
                userId, actualTimeout.TotalSeconds);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var queryDbContext = scope.ServiceProvider.GetRequiredService<QueryDbContext>();

                    var user = await queryDbContext.Users
                        .Find(u => u.Id == userId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (user != null)
                    {
                        _logger.LogDebug("User {UserId} found in MongoDB", userId);
                        return true;
                    }

                    await Task.Delay(_pollInterval, cancellationToken);
                }

                _logger.LogWarning("Timeout waiting for user {UserId} to appear in MongoDB after {Timeout}s", 
                    userId, actualTimeout.TotalSeconds);
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout waiting for user {UserId} to appear in MongoDB after {Timeout}s", 
                    userId, actualTimeout.TotalSeconds);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for user {UserId} in MongoDB", userId);
                return false;
            }
        }

        /// <summary>
        /// Waits for a user to be marked as deleted in MongoDB
        /// </summary>
        /// <param name="userId">ID of the user to wait for deletion</param>
        /// <param name="timeout">Maximum time to wait (default: 30s)</param>
        /// <returns>True if the user is marked as deleted within the timeout period</returns>
        public async Task<bool> WaitForUserDeletedAsync(Guid userId, TimeSpan? timeout = null)
        {
            return await WaitForUserDeletionAsync(userId, timeout);
        }

        /// <summary>
        /// Waits for a user to be marked as deleted in MongoDB
        /// </summary>
        /// <param name="userId">ID of the user to wait for deletion</param>
        /// <param name="timeout">Maximum time to wait (default: 30s)</param>
        /// <returns>True if the user is marked as deleted within the timeout period</returns>
        public async Task<bool> WaitForUserDeletionAsync(Guid userId, TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? _defaultTimeout;
            var cancellationToken = new CancellationTokenSource(actualTimeout).Token;

            _logger.LogInformation("Waiting for user {UserId} to be marked as deleted in MongoDB (timeout: {Timeout}s)", 
                userId, actualTimeout.TotalSeconds);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var queryDbContext = scope.ServiceProvider.GetRequiredService<QueryDbContext>();

                    var user = await queryDbContext.Users
                        .Find(u => u.Id == userId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (user != null)
                    {
                        _logger.LogInformation("User {UserId} found in MongoDB - IsDeleted: {IsDeleted}, DeletedAt: {DeletedAt}", 
                            userId, user.IsDeleted, user.DeletedAt);
                        
                        if (user.IsDeleted && user.DeletedAt.HasValue)
                        {
                            _logger.LogInformation("User {UserId} marked as deleted in MongoDB", userId);
                            return true;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("User {UserId} not found in MongoDB", userId);
                    }

                    await Task.Delay(_pollInterval, cancellationToken);
                }

                _logger.LogWarning("Timeout waiting for user {UserId} to be marked as deleted in MongoDB after {Timeout}s", 
                    userId, actualTimeout.TotalSeconds);
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout waiting for user {UserId} to be marked as deleted in MongoDB after {Timeout}s", 
                    userId, actualTimeout.TotalSeconds);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for user {UserId} deletion in MongoDB", userId);
                return false;
            }
        }

        /// <summary>
        /// Waits for a specific field condition to be met for a user in MongoDB
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="fieldChecker">Function that checks if the field condition is met</param>
        /// <param name="timeout">Maximum time to wait (default: 30s)</param>
        /// <returns>True if the condition is met within the timeout period</returns>
        public async Task<bool> WaitForUserFieldUpdateAsync(Guid userId, Func<UserQueryDocument, bool> fieldChecker, TimeSpan? timeout = null)
        {
            var actualTimeout = timeout ?? _defaultTimeout;
            var cancellationToken = new CancellationTokenSource(actualTimeout).Token;

            _logger.LogDebug("Waiting for user {UserId} field update in MongoDB (timeout: {Timeout}s)", 
                userId, actualTimeout.TotalSeconds);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var queryDbContext = scope.ServiceProvider.GetRequiredService<QueryDbContext>();

                    var user = await queryDbContext.Users
                        .Find(u => u.Id == userId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (user != null && fieldChecker(user))
                    {
                        _logger.LogDebug("User {UserId} field condition met in MongoDB", userId);
                        return true;
                    }

                    await Task.Delay(_pollInterval, cancellationToken);
                }

                _logger.LogWarning("Timeout waiting for user {UserId} field update in MongoDB after {Timeout}s", 
                    userId, actualTimeout.TotalSeconds);
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout waiting for user {UserId} field update in MongoDB after {Timeout}s", 
                    userId, actualTimeout.TotalSeconds);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for user {UserId} field update in MongoDB", userId);
                return false;
            }
        }

        /// <summary>
        /// Checks if a user exists in MongoDB without waiting
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <returns>True if the user exists, false otherwise</returns>
        public async Task<bool> UserExistsInMongoDbAsync(Guid userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var queryDbContext = scope.ServiceProvider.GetRequiredService<QueryDbContext>();

                var user = await queryDbContext.Users
                    .Find(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} exists in MongoDB", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets a user from MongoDB if it exists
        /// </summary>
        /// <param name="userId">ID of the user to retrieve</param>
        /// <returns>The user document if found, null otherwise</returns>
        public async Task<UserQueryDocument?> GetUserFromMongoDbAsync(Guid userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var queryDbContext = scope.ServiceProvider.GetRequiredService<QueryDbContext>();

                return await queryDbContext.Users
                    .Find(u => u.Id == userId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId} from MongoDB", userId);
                return null;
            }
        }
    }
}
