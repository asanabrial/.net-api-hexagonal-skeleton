using AutoMapper;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using HexagonalSkeleton.Infrastructure.Services;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.GeoJsonObjectModel;

namespace HexagonalSkeleton.Infrastructure.Adapters.Query
{
    /// <summary>
    /// Implementation of the IUserReadRepository using MongoDB for optimized read operations
    /// Following Hexagonal Architecture with adapters and ports
    /// Also implements specialized interfaces following Interface Segregation Principle
    /// </summary>
    public class UserReadRepositoryMongoAdapter : IUserReadRepository, IUserExistenceChecker, IUserSearchService, IUserReader
    {
        private readonly QueryDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMongoFilterBuilder _filterBuilder;
        private readonly IMongoSortBuilder _sortBuilder;

        public UserReadRepositoryMongoAdapter(
            QueryDbContext dbContext, 
            IMapper mapper,
            IMongoFilterBuilder filterBuilder,
            IMongoSortBuilder sortBuilder)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _filterBuilder = filterBuilder ?? throw new ArgumentNullException(nameof(filterBuilder));
            _sortBuilder = sortBuilder ?? throw new ArgumentNullException(nameof(sortBuilder));
        }

        /// <summary>
        /// Checks if MongoDB operations are available (not in test environment)
        /// </summary>
        private bool IsMongoDbAvailable => _dbContext.Users != null;

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        public async Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (!IsMongoDbAvailable)
                return null;

            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, userId) &
                         Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            
            var document = await _dbContext.Users!
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            return document != null ? _mapper.Map<User>(document) : null;
        }

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Email, email) &
                         Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            
            var document = await _dbContext.Users
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            return document != null ? _mapper.Map<User>(document) : null;
        }

        /// <summary>
        /// Gets users based on a specification with pagination
        /// </summary>
        public async Task<PagedResult<User>> GetUsersAsync(
            ISpecification<User> specification, 
            PaginationParams pagination,
            CancellationToken cancellationToken = default)
        {
            // Convert the specification to a MongoDB filter using the injected service
            var filter = _filterBuilder.ConvertSpecificationToMongoFilter(specification);
            
            // Add not deleted filter
            filter &= Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);

            // Get total count for pagination
            var totalCount = await _dbContext.Users
                .CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            // Apply pagination and sorting
            var sortDefinition = Builders<UserQueryDocument>.Sort
                .Descending(u => u.CreatedAt);

            var documents = await _dbContext.Users
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(pagination.PageSize * (pagination.PageNumber - 1))
                .Limit(pagination.PageSize)
                .ToListAsync(cancellationToken);

            // Map to domain entities
            var users = _mapper.Map<List<User>>(documents);

            // Create paged result
            return new PagedResult<User>(users, (int)totalCount, pagination);
        }

        /// <summary>
        /// Finds nearby users with radius and additional filters
        /// </summary>
        public async Task<List<User>> FindNearbyUsersAsync(
            double latitude, 
            double longitude,
            double radiusInKm, 
            bool adultOnly,
            CancellationToken cancellationToken = default)
        {
            // Create geospatial query using MongoDB's GeoJsonPoint
            var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(longitude, latitude));
            
            var geoNearFilter = Builders<UserQueryDocument>.Filter
                .NearSphere(u => u.Location, point, radiusInKm * 1000);
            
            // Combine with other filters
            var filter = geoNearFilter;
            
            if (adultOnly)
            {
                filter &= Builders<UserQueryDocument>.Filter.Gte(u => u.Age, 18);
            }
            
            // Add not deleted filter
            filter &= Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);

            var documents = await _dbContext.Users
                .Find(filter)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<User>>(documents);
        }
        
        // Implementation of IUserReadRepository methods
        
        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await GetUserAsync(id, cancellationToken);
        }

        public async Task<User?> GetByIdUnfilteredAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!IsMongoDbAvailable)
                return null;

            // Query WITHOUT the IsDeleted filter for management purposes
            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, id);
            
            var document = await _dbContext.Users!
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            return document != null ? _mapper.Map<User>(document) : null;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await GetUserByEmailAsync(email, cancellationToken);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            var documents = await _dbContext.Users
                .Find(filter)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<User>>(documents);
        }

        public async Task<PagedResult<User>> GetUsersAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            // Create a default filter for active users
            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            
            // Get total count for pagination
            var totalCount = await _dbContext.Users
                .CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            // Apply sorting using the injected service
            var sortDefinition = _sortBuilder.GetSortDefinition(pagination.SortBy, pagination.SortDirection == "desc");

            var documents = await _dbContext.Users
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(pagination.PageSize * (pagination.PageNumber - 1))
                .Limit(pagination.PageSize)
                .ToListAsync(cancellationToken);

            // Map to domain entities
            var users = _mapper.Map<List<User>>(documents);

            // Create paged result
            return new PagedResult<User>(users, (int)totalCount, pagination);
        }

        public async Task<PagedResult<User>> SearchUsersAsync(PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetUsersAsync(pagination, cancellationToken);
                
            var searchTermLower = searchTerm.ToLowerInvariant();
            
            // Build search filter
            var builder = Builders<UserQueryDocument>.Filter;
            var searchFilter = builder.Or(
                builder.Regex(u => u.FullName.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTermLower, "i")),
                builder.Regex(u => u.FullName.LastName, new MongoDB.Bson.BsonRegularExpression(searchTermLower, "i")),
                builder.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(searchTermLower, "i")),
                builder.Regex(u => u.PhoneNumber, new MongoDB.Bson.BsonRegularExpression(searchTermLower, "i"))
            );
            
            // Combine with active filter
            var filter = builder.And(builder.Eq(u => u.IsDeleted, false), searchFilter);
            
            // Get total count
            var totalCount = await _dbContext.Users
                .CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                
            // Apply sorting using the injected service
            var sortDefinition = _sortBuilder.GetSortDefinition(pagination.SortBy, pagination.SortDirection == "desc");
            
            // Execute query with pagination
            var documents = await _dbContext.Users
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(pagination.PageSize * (pagination.PageNumber - 1))
                .Limit(pagination.PageSize)
                .ToListAsync(cancellationToken);
                
            // Map and return
            var users = _mapper.Map<List<User>>(documents);
            return new PagedResult<User>(users, (int)totalCount, pagination);
        }

        public async Task<List<User>> GetUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            var filter = _filterBuilder.ConvertSpecificationToMongoFilter(specification);
            filter &= Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            
            var documents = await _dbContext.Users
                .Find(filter)
                .ToListAsync(cancellationToken);
                
            return _mapper.Map<List<User>>(documents);
        }

        public async Task<int> CountUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            if (!IsMongoDbAvailable)
                return 0;

            var filter = _filterBuilder.ConvertSpecificationToMongoFilter(specification);
            filter &= Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            
            var count = await _dbContext.Users!
                .CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                
            return (int)count;
        }

        public async Task<bool> AnyUsersAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
        {
            if (!IsMongoDbAvailable)
                return false;

            var filter = _filterBuilder.ConvertSpecificationToMongoFilter(specification);
            filter &= Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
            
            var count = await _dbContext.Users!
                .CountDocumentsAsync(filter, 
                    new CountOptions { Limit = 1 },
                    cancellationToken);
                
            return count > 0;
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            // Return false if Users collection is not available (test environment)
            if (_dbContext.Users == null)
                return false;

            var normalizedEmail = email.ToLowerInvariant();
            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Email, normalizedEmail) & 
                        Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
                        
            var count = await _dbContext.Users!
                .CountDocumentsAsync(filter, 
                    new CountOptions { Limit = 1 },
                    cancellationToken);
                
            return count > 0;
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            if (!IsMongoDbAvailable)
                return false;

            var normalizedPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.PhoneNumber, normalizedPhone) & 
                        Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
                        
            var count = await _dbContext.Users!
                .CountDocumentsAsync(filter, 
                    new CountOptions { Limit = 1 },
                    cancellationToken);
                
            return count > 0;
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!IsMongoDbAvailable)
                return false;

            var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, id) & 
                        Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
                        
            var count = await _dbContext.Users!
                .CountDocumentsAsync(filter, 
                    new CountOptions { Limit = 1 },
                    cancellationToken);
                
            return count > 0;
        }


    }
}
