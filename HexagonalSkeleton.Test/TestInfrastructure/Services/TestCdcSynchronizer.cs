using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

namespace HexagonalSkeleton.Test.TestInfrastructure.Services
{
    /// <summary>
    /// Test service that simulates CDC synchronization between PostgreSQL and MongoDB
    /// Provides direct synchronization for integration tests
    /// </summary>
    public class TestCdcSynchronizer
    {
        private readonly IUserWriteRepository _writeRepository;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ILogger<TestCdcSynchronizer> _logger;

        public TestCdcSynchronizer(
            IUserWriteRepository writeRepository,
            IMongoDatabase mongoDatabase,
            ILogger<TestCdcSynchronizer> logger)
        {
            _writeRepository = writeRepository;
            _mongoDatabase = mongoDatabase;
            _logger = logger;
        }

        /// <summary>
        /// Synchronizes a user from PostgreSQL to MongoDB (simulates CDC for user creation)
        /// </summary>
        public async Task SynchronizeUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Test CDC: Synchronizing user {UserId} from PostgreSQL to MongoDB", userId);

                // Get user from PostgreSQL (write repository)
                var user = await _writeRepository.GetByIdUnfilteredAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("Test CDC: User {UserId} not found in PostgreSQL", userId);
                    return;
                }

                // Convert to MongoDB document format matching UserQueryDocument structure
                var fullNameDocument = new BsonDocument
                {
                    ["firstName"] = user.FullName.FirstName,
                    ["lastName"] = user.FullName.LastName,
                    ["displayName"] = $"{user.FullName.FirstName} {user.FullName.LastName}"
                };

                var locationDocument = new BsonDocument
                {
                    ["latitude"] = user.Location.Latitude,
                    ["longitude"] = user.Location.Longitude,
                    ["address"] = string.Empty,
                    ["city"] = string.Empty,
                    ["country"] = string.Empty,
                    ["coordinates"] = new BsonArray { user.Location.Longitude, user.Location.Latitude }
                };

                var userDocument = new BsonDocument
                {
                    ["_id"] = user.Id.ToString(),
                    ["fullName"] = fullNameDocument,
                    ["email"] = user.Email.Value,
                    ["phoneNumber"] = user.PhoneNumber?.Value,
                    ["passwordHash"] = user.PasswordHash,
                    ["passwordSalt"] = user.PasswordSalt,
                    ["birthdate"] = user.Birthdate,
                    ["location"] = locationDocument,
                    ["aboutMe"] = string.IsNullOrEmpty(user.AboutMe) ? BsonNull.Value : (BsonValue)user.AboutMe,
                    ["isDeleted"] = user.IsDeleted,
                    ["lastLogin"] = user.LastLogin,
                    ["createdAt"] = user.CreatedAt,
                    ["updatedAt"] = user.UpdatedAt
                };

                // Insert/Update in MongoDB
                var collection = _mongoDatabase.GetCollection<BsonDocument>("users");
                await collection.ReplaceOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", user.Id.ToString()),
                    userDocument,
                    new ReplaceOptions { IsUpsert = true },
                    cancellationToken);

                _logger.LogInformation("Test CDC: Successfully synchronized user {UserId} to MongoDB", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test CDC: Failed to synchronize user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Synchronizes user deletion from PostgreSQL to MongoDB (simulates CDC for logical deletion)
        /// </summary>
        public async Task SynchronizeUserDeletionAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Test CDC: Synchronizing user deletion {UserId} from PostgreSQL to MongoDB", userId);

                // Get updated user from PostgreSQL (should have IsDeleted = true)
                var user = await _writeRepository.GetByIdUnfilteredAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("Test CDC: User {UserId} not found in PostgreSQL", userId);
                    return;
                }

                // Update MongoDB document to mark as deleted
                var collection = _mongoDatabase.GetCollection<BsonDocument>("users");
                var updateResult = await collection.UpdateOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", user.Id.ToString()),
                    Builders<BsonDocument>.Update
                        .Set("isDeleted", true)
                        .Set("updatedAt", user.UpdatedAt),
                    cancellationToken: cancellationToken);

                if (updateResult.MatchedCount == 0)
                {
                    _logger.LogWarning("Test CDC: User {UserId} not found in MongoDB for deletion", userId);
                }
                else
                {
                    _logger.LogInformation("Test CDC: Successfully synchronized user deletion {UserId} to MongoDB", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test CDC: Failed to synchronize user deletion {UserId}", userId);
                throw;
            }
        }
    }
}
