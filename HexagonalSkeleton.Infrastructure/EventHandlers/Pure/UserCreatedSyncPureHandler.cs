using HexagonalSkeleton.Application.Ports;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.EventHandlers.Pure
{
    /// <summary>
    /// Pure CQRS sync handler - synchronizes command store changes to query store
    /// Implements eventual consistency between PostgreSQL (command) and MongoDB (query)
    /// Handles UserCreatedEvent to maintain data consistency across CQRS boundaries
    /// No dependency on MediatR - implements our pure interface
    /// </summary>
    public class UserCreatedSyncPureHandler : IDomainEventHandler<UserCreatedEvent>
    {
        private readonly QueryDbContext _queryContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCreatedSyncPureHandler> _logger;

        public UserCreatedSyncPureHandler(
            QueryDbContext queryContext,
            IMapper mapper,
            ILogger<UserCreatedSyncPureHandler> logger)
        {
            _queryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(UserCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Synchronizing user creation to query store: {UserId}", domainEvent.UserId);

                // Check if user already exists in query store
                var existingUser = await _queryContext.Users
                    .Find(u => u.Id == domainEvent.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingUser != null)
                {
                    _logger.LogWarning("User {UserId} already exists in query store. Skipping sync.", domainEvent.UserId);
                    return;
                }

                // Create new user document for query store
                var userDocument = new UserQueryDocument
                {
                    Id = domainEvent.UserId,
                    Email = domainEvent.Email,
                    FullName = new FullNameDocument
                    {
                        FirstName = domainEvent.Name,
                        LastName = domainEvent.Surname
                    },
                    PhoneNumber = domainEvent.PhoneNumber,
                    Birthdate = null, // Will be set when user updates profile
                    Age = null, // Will be calculated when birthdate is set
                    AboutMe = string.Empty,
                    Location = new LocationDocument(), // Empty location initially
                    IsDeleted = false,
                    CreatedAt = domainEvent.CreatedAt,
                    UpdatedAt = null,
                    LastLogin = null,
                    SearchTerms = new List<string> { domainEvent.Email, domainEvent.Name, domainEvent.Surname },
                    ProfileCompleteness = 0.3 // Basic info provided (email, name, phone)
                };

                // Insert into query store
                await _queryContext.Users.InsertOneAsync(userDocument, cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully synchronized user {UserId} to query store", domainEvent.UserId);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Handle duplicate key gracefully (race condition)
                _logger.LogWarning("Duplicate key detected while syncing user {UserId}. Another process may have already synced this user.", domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize user {UserId} to query store", domainEvent.UserId);
                
                // In a production system, you might want to:
                // 1. Add to a retry queue
                // 2. Publish a compensation event
                // 3. Alert monitoring systems
                throw; // Re-throw to ensure the domain event handling fails and can be retried
            }
        }
    }
}
