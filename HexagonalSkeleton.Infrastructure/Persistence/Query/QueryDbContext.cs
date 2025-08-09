using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using Microsoft.Extensions.Configuration;

namespace HexagonalSkeleton.Infrastructure.Persistence.Query
{
    /// <summary>
    /// MongoDB context for read operations
    /// Optimized for high-performance queries and analytics
    /// Follows MongoDB best practices for read-heavy workloads
    /// </summary>
    public class QueryDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;

        public QueryDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("HexagonalSkeletonRead");
            var databaseName = configuration["MongoDb:DatabaseName"] ?? "HexagonalSkeletonRead";

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
            
            // Ensure indexes are created
            ConfigureIndexes();
        }

        public QueryDbContext(IMongoClient client, string databaseName)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("Database name cannot be empty", nameof(databaseName));
            
            _client = client;
            _database = client.GetDatabase(databaseName);
            
            // Only configure indexes if not in test environment
            // Check database name directly to avoid NullReferenceException
            if (!databaseName.Contains("Test", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureIndexes();
            }
        }

        /// <summary>
        /// Checks if we're running in a test environment to avoid MongoDB operations
        /// </summary>
        private bool IsTestEnvironment()
        {
            // Check if we're in a test environment (database name contains "Test" or we're using in-memory)
            return _database?.DatabaseNamespace?.DatabaseName?.Contains("Test", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Users collection optimized for read operations
        /// </summary>
        public IMongoCollection<UserQueryDocument> Users => 
            _database?.GetCollection<UserQueryDocument>("users") 
            ?? throw new InvalidOperationException("Database is not properly initialized");

        /// <summary>
        /// Configure MongoDB indexes for optimal query performance
        /// </summary>
        private void ConfigureIndexes()
        {
            // Skip index creation if database is not properly initialized (test environment)
            if (_database?.DatabaseNamespace?.DatabaseName == null)
                return;

            try
            {
                var usersCollection = Users; // Now guaranteed to be non-null or throw
                
                // Create indexes for common query patterns
                var indexKeysDefinition = Builders<UserQueryDocument>.IndexKeys;

                // Email index (unique)
                var emailIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Ascending(x => x.Email),
                    new CreateIndexOptions { Unique = true, Sparse = true });

                // Phone number index (unique)
                var phoneIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Ascending(x => x.PhoneNumber),
                    new CreateIndexOptions { Unique = true, Sparse = true });

                // Full name text search index
                var fullNameTextIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Text(x => x.FullName.FirstName)
                        .Text(x => x.FullName.LastName)
                        .Text(x => x.FullName.DisplayName),
                    new CreateIndexOptions { Name = "fulltext_search" });

                // Search terms index for advanced searching
                var searchTermsIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Ascending(x => x.SearchTerms));

                // Active users index for filtering (using IsDeleted)
                var activeIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Ascending(x => x.IsDeleted));

                // Age index for demographic queries
                var ageIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Ascending(x => x.Age));

                // Location-based 2dsphere index for geospatial queries
                var locationIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Geo2DSphere($"{nameof(UserQueryDocument.Location).ToLowerInvariant()}.coordinates"),
                    new CreateIndexOptions { Name = "location_2dsphere" });

                // Compound index for common filtering patterns
                var compoundActiveEmailIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Ascending(x => x.IsDeleted).Ascending(x => x.Email));

                // Last login index for activity analysis
                var lastLoginIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Descending(x => x.LastLogin));

                // Created at index for time-based queries
                var createdAtIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Descending(x => x.CreatedAt));

                // Profile completeness index for analytics
                var profileCompletenessIndex = new CreateIndexModel<UserQueryDocument>(
                    indexKeysDefinition.Descending(x => x.ProfileCompleteness));

                usersCollection.Indexes.CreateMany(new[]
                {
                    emailIndex,
                    phoneIndex,
                    fullNameTextIndex,
                    searchTermsIndex,
                    activeIndex,
                    ageIndex,
                    locationIndex,
                    compoundActiveEmailIndex,
                    lastLoginIndex,
                    createdAtIndex,
                    profileCompletenessIndex
                });
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
            {
                // Indexes already exist, this is expected during development
                // Silently continue - no logging needed for this expected scenario
            }
            catch (Exception)
            {
                // Ignore index creation errors in test environments
                // Production environments should have proper error handling
            }
        }

        /// <summary>
        /// Get database statistics for monitoring
        /// </summary>
        public async Task<DatabaseStatsResult> GetDatabaseStatsAsync()
        {
            var command = new MongoDB.Bson.BsonDocument("dbStats", 1);
            return await _database.RunCommandAsync<DatabaseStatsResult>(command);
        }

        /// <summary>
        /// Ensure the database connection is healthy
        /// </summary>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var command = new MongoDB.Bson.BsonDocument("ping", 1);
                await _database.RunCommandAsync<MongoDB.Bson.BsonDocument>(command);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Result for database statistics
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DatabaseStatsResult
    {
        [BsonElement("db")]
        public string Db { get; set; } = string.Empty;
        
        [BsonElement("collections")]
        public long Collections { get; set; }
        
        [BsonElement("views")]
        public long Views { get; set; }
        
        [BsonElement("objects")]
        public long Objects { get; set; }
        
        [BsonElement("avgObjSize")]
        public double AvgObjSize { get; set; }
        
        [BsonElement("dataSize")]
        public long DataSize { get; set; }
        
        [BsonElement("storageSize")]
        public long StorageSize { get; set; }
        
        [BsonElement("indexes")]
        public long Indexes { get; set; }
        
        [BsonElement("indexSize")]
        public long IndexSize { get; set; }
        
        [BsonElement("ok")]
        public double Ok { get; set; }
    }
}
