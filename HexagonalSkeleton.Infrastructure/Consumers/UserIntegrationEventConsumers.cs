using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.Consumers
{
    /// <summary>
    /// Simple consumer for UserCreated events
    /// Single responsibility: sync user data to read model
    /// </summary>
    public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        private readonly QueryDbContext _queryDbContext;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(QueryDbContext queryDbContext, ILogger<UserCreatedConsumer> logger)
        {
            _queryDbContext = queryDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Syncing user {UserId} to read model", message.UserId);

            try
            {
                // Simple check - avoid duplicates
                var existingUser = await _queryDbContext.Users
                    .Find(u => u.Id == message.UserId)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    _logger.LogWarning("User {UserId} already exists in read model", message.UserId);
                    return;
                }

                // Create simple user document with essential data only
                var userDocument = new UserQueryDocument
                {
                    Id = message.UserId,
                    Email = message.Email,
                    FullName = new FullNameDocument
                    {
                        FirstName = message.FirstName,
                        LastName = message.LastName
                    },
                    PhoneNumber = message.PhoneNumber,
                    CreatedAt = message.CreatedAt,
                    LastLogin = null,
                    Location = new LocationDocument { Latitude = 0, Longitude = 0 }, // Default empty location
                    SearchTerms = new List<string> 
                    { 
                        message.FirstName.ToLowerInvariant(),
                        message.LastName.ToLowerInvariant(),
                        message.Email.ToLowerInvariant()
                    },
                    Age = null,
                    IsDeleted = false,
                    ProfileCompleteness = 0.3, // Basic info
                    UpdatedAt = null
                };

                await _queryDbContext.Users.InsertOneAsync(userDocument);
                _logger.LogInformation("User {UserId} successfully synced to read model", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync user {UserId} to read model", message.UserId);
                throw;
            }
        }
    }

    /// <summary>
    /// Simple consumer for UserLoggedIn events
    /// Single responsibility: update last login time
    /// </summary>
    public class UserLoggedInConsumer : IConsumer<UserLoggedInIntegrationEvent>
    {
        private readonly QueryDbContext _queryDbContext;
        private readonly ILogger<UserLoggedInConsumer> _logger;

        public UserLoggedInConsumer(QueryDbContext queryDbContext, ILogger<UserLoggedInConsumer> logger)
        {
            _queryDbContext = queryDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Updating last login for user {UserId}", message.UserId);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, message.UserId);
                var update = Builders<UserQueryDocument>.Update.Set(u => u.LastLogin, message.LoginTime);

                await _queryDbContext.Users.UpdateOneAsync(filter, update);
                _logger.LogInformation("Last login updated for user {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update last login for user {UserId}", message.UserId);
                throw;
            }
        }
    }
}
