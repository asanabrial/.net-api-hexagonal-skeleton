using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Application.Services;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Infrastructure.Consumers
{
    /// <summary>
    /// Consumer for user integration events that updates the read model in MongoDB
    /// </summary>
    public class UserCreatedConsumer : IConsumer<HexagonalSkeleton.Application.IntegrationEvents.UserCreatedIntegrationEvent>
    {
        private readonly QueryDbContext _queryDbContext;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(
            QueryDbContext queryDbContext,
            ILogger<UserCreatedConsumer> logger)
        {
            _queryDbContext = queryDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<HexagonalSkeleton.Application.IntegrationEvents.UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received UserCreatedIntegrationEvent for {Email}", message.Email);

            try
            {
                // Check if user already exists
                var existingUser = await _queryDbContext.Users
                    .Find(u => u.Email == message.Email)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists in read model", message.Email);
                    return;
                }

                var UserQueryDocument = new UserQueryDocument
                {
                    Id = message.UserId,
                    Email = message.Email,
                    FullName = new FullNameDocument
                    {
                        FirstName = message.FirstName,
                        LastName = message.LastName
                    },
                    PhoneNumber = message.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    
                    // Create GeoJsonPoint for MongoDB - default to (0,0) if not available
                    Location = new LocationDocument
                    {
                        Latitude = message.Latitude ?? 0,
                        Longitude = message.Longitude ?? 0
                    },
                    
                    // Set search terms for improved search functionality
                    SearchTerms = new List<string>
                    {
                        message.FirstName.ToLowerInvariant(),
                        message.LastName.ToLowerInvariant(),
                        message.Email.ToLowerInvariant()
                    },
                    
                    // Set other properties
                    Age = message.Age ?? 0,
                    IsDeleted = false,
                    ProfileCompleteness = 0.5 // Basic profile completion
                };

                await _queryDbContext.Users.InsertOneAsync(UserQueryDocument);
                _logger.LogInformation("User {Email} successfully added to read model", message.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {Email} to read model", message.Email);
                throw;
            }
        }
    }

    /// <summary>
    /// Consumer for user profile update integration events
    /// </summary>
    public class UserProfileUpdatedConsumer : IConsumer<HexagonalSkeleton.Application.IntegrationEvents.UserProfileUpdatedIntegrationEvent>
    {
        private readonly QueryDbContext _queryDbContext;
        private readonly ILogger<UserProfileUpdatedConsumer> _logger;

        public UserProfileUpdatedConsumer(
            QueryDbContext queryDbContext,
            ILogger<UserProfileUpdatedConsumer> logger)
        {
            _queryDbContext = queryDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<HexagonalSkeleton.Application.IntegrationEvents.UserProfileUpdatedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received UserProfileUpdatedIntegrationEvent for user {UserId}", message.UserId);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, message.UserId);
                var existingUser = await _queryDbContext.Users.Find(filter).FirstOrDefaultAsync();

                if (existingUser == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found in read model", message.UserId);
                    
                    // Create a new user document if not exists (data consistency)
                    var userDocument = new UserQueryDocument
                    {
                        Id = message.UserId,
                        Email = message.Email,
                        FullName = new FullNameDocument
                        {
                            FirstName = message.FirstName,
                            LastName = message.LastName
                        },
                        CreatedAt = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow,
                        
                        // Create proper location document structure
                        Location = new LocationDocument
                        {
                            Latitude = message.Latitude ?? 0,
                            Longitude = message.Longitude ?? 0
                        },
                        
                        // Set search terms for improved search functionality
                        SearchTerms = new List<string>
                        {
                            message.FirstName.ToLowerInvariant(),
                            message.LastName.ToLowerInvariant(),
                            message.Email.ToLowerInvariant()
                        },
                        
                        // Set other properties
                        Age = message.Age ?? 0,
                        IsDeleted = false,
                        ProfileCompleteness = 0.5,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _queryDbContext.Users.InsertOneAsync(userDocument);
                    _logger.LogInformation("Created new user {UserId} in read model from update event", message.UserId);
                    return;
                }

                var update = Builders<UserQueryDocument>.Update
                    .Set(u => u.FullName.FirstName, message.FirstName)
                    .Set(u => u.FullName.LastName, message.LastName)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                await _queryDbContext.Users.UpdateOneAsync(filter, update);
                _logger.LogInformation("User {UserId} profile successfully updated in read model", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} in read model", message.UserId);
                throw;
            }
        }
    }

    /// <summary>
    /// MassTransit consumer for UserLoggedInIntegrationEvent
    /// </summary>
    public class UserLoggedInConsumer : IIntegrationEventHandler<UserLoggedInIntegrationEvent>
    {
        private readonly HexagonalSkeleton.Domain.Services.IUserSyncService _syncService;
        private readonly ILogger<UserLoggedInConsumer> _logger;

        public UserLoggedInConsumer(
            HexagonalSkeleton.Domain.Services.IUserSyncService syncService,
            ILogger<UserLoggedInConsumer> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
        {
            var message = context.Message;
            
            try
            {
                _logger.LogInformation("Processing UserLoggedInIntegrationEvent for user {UserId}", message.UserId);

                // Update only the last login time in MongoDB
                await _syncService.UpdateUserLastLoginAsync(message.UserId, message.LoginTime, context.CancellationToken);

                _logger.LogInformation("Successfully processed UserLoggedInIntegrationEvent for user {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserLoggedInIntegrationEvent for user {UserId}", message.UserId);
                throw;
            }
        }
    }

    /// <summary>
    /// MassTransit consumer for UserDeletedIntegrationEvent
    /// </summary>
    public class UserDeletedConsumer : IIntegrationEventHandler<UserDeletedIntegrationEvent>
    {
        private readonly HexagonalSkeleton.Domain.Services.IUserSyncService _syncService;
        private readonly ILogger<UserDeletedConsumer> _logger;

        public UserDeletedConsumer(
            HexagonalSkeleton.Domain.Services.IUserSyncService syncService,
            ILogger<UserDeletedConsumer> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedIntegrationEvent> context)
        {
            var message = context.Message;
            
            try
            {
                _logger.LogInformation("Processing UserDeletedIntegrationEvent for user {UserId}", message.UserId);

                if (message.IsHardDelete)
                {
                    // Remove user completely from query store
                    await _syncService.RemoveUserAsync(message.UserId, context.CancellationToken);
                }
                else
                {
                    // Soft delete - mark as inactive
                    await _syncService.DeactivateUserAsync(message.UserId, context.CancellationToken);
                }

                _logger.LogInformation("Successfully processed UserDeletedIntegrationEvent for user {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserDeletedIntegrationEvent for user {UserId}", message.UserId);
                throw;
            }
        }
    }
}
