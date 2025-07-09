using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace HexagonalSkeleton.Infrastructure.Adapters.Query
{
    /// <summary>
    /// Query repository adapter for user read operations
    /// Implements the outbound port for data querying
    /// Follows CQRS pattern - only handles read operations
    /// Uses MongoDB for high-performance queries
    /// </summary>
    public class UserQueryRepository : IUserQueryRepository
    {
        private readonly QueryDbContext _queryContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserQueryRepository> _logger;

        public UserQueryRepository(
            QueryDbContext queryContext,
            IMapper mapper,
            ILogger<UserQueryRepository> logger)
        {
            _queryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<UserQueryDto>> GetAllAsync(
            PaginationParams paginationParams, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all users with pagination: Skip={Skip}, Take={Take}", 
                paginationParams.Skip, paginationParams.Take);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
                
                var totalCount = await _queryContext.Users.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                
                var users = await _queryContext.Users
                    .Find(filter)
                    .Sort(Builders<UserQueryDocument>.Sort.Descending(u => u.CreatedAt))
                    .Skip(paginationParams.Skip)
                    .Limit(paginationParams.Take)
                    .ToListAsync(cancellationToken);

                var userDtos = _mapper.Map<List<UserQueryDto>>(users);

                return new PagedResult<UserQueryDto>(userDtos, (int)totalCount, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<UserQueryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", id);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.And(
                    Builders<UserQueryDocument>.Filter.Eq(u => u.Id, id),
                    Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false)
                );

                var user = await _queryContext.Users
                    .Find(filter)
                    .FirstOrDefaultAsync(cancellationToken);

                return user != null ? _mapper.Map<UserQueryDto>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                throw;
            }
        }

        public async Task<UserQueryDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user by email: {Email}", email);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.And(
                    Builders<UserQueryDocument>.Filter.Eq(u => u.Email, email.ToLowerInvariant()),
                    Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false)
                );

                var user = await _queryContext.Users
                    .Find(filter)
                    .FirstOrDefaultAsync(cancellationToken);

                return user != null ? _mapper.Map<UserQueryDto>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking if user exists by email: {Email}", email);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.And(
                    Builders<UserQueryDocument>.Filter.Eq(u => u.Email, email.ToLowerInvariant()),
                    Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false)
                );

                var count = await _queryContext.Users.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists by email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking if user exists by phone: {PhoneNumber}", phoneNumber);

            try
            {
                var filter = Builders<UserQueryDocument>.Filter.And(
                    Builders<UserQueryDocument>.Filter.Eq(u => u.PhoneNumber, phoneNumber),
                    Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false)
                );

                var count = await _queryContext.Users.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user exists by phone: {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task<PagedResult<UserQueryDto>> SearchAsync(
            string searchTerm, 
            PaginationParams paginationParams, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);

            try
            {
                var filterBuilder = Builders<UserQueryDocument>.Filter;
                var textFilter = filterBuilder.Text(searchTerm);
                var activeFilter = filterBuilder.Eq(u => u.IsDeleted, false);
                var combinedFilter = filterBuilder.And(textFilter, activeFilter);

                var totalCount = await _queryContext.Users.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

                var users = await _queryContext.Users
                    .Find(combinedFilter)
                    .Sort(Builders<UserQueryDocument>.Sort.MetaTextScore("score"))
                    .Skip(paginationParams.Skip)
                    .Limit(paginationParams.Take)
                    .ToListAsync(cancellationToken);

                var userDtos = _mapper.Map<List<UserQueryDto>>(users);

                return new PagedResult<UserQueryDto>(userDtos, (int)totalCount, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<PagedResult<UserQueryDto>> GetUsersByLocationAsync(
            double latitude, 
            double longitude, 
            double radiusKm,
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting users by location: {Lat}, {Lng}, Radius: {Radius}km", 
                latitude, longitude, radiusKm);

            try
            {
                var filterBuilder = Builders<UserQueryDocument>.Filter;
                var geoFilter = filterBuilder.GeoWithinCenterSphere(
                    u => u.Location,
                    longitude, latitude, radiusKm / 6378.1); // Earth's radius in km
                var activeFilter = filterBuilder.Eq(u => u.IsDeleted, false);
                var combinedFilter = filterBuilder.And(geoFilter, activeFilter);

                var totalCount = await _queryContext.Users.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

                var users = await _queryContext.Users
                    .Find(combinedFilter)
                    .Sort(Builders<UserQueryDocument>.Sort.Descending(u => u.CreatedAt))
                    .Skip(paginationParams.Skip)
                    .Limit(paginationParams.Take)
                    .ToListAsync(cancellationToken);

                var userDtos = _mapper.Map<List<UserQueryDto>>(users);

                return new PagedResult<UserQueryDto>(userDtos, (int)totalCount, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by location");
                throw;
            }
        }

        public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user statistics");

            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var weekAgo = today.AddDays(-7);
                var monthAgo = today.AddMonths(-1);

                var activeFilter = Builders<UserQueryDocument>.Filter.Eq(u => u.IsDeleted, false);
                
                var totalUsers = await _queryContext.Users.CountDocumentsAsync(activeFilter, cancellationToken: cancellationToken);
                
                var newUsersToday = await _queryContext.Users.CountDocumentsAsync(
                    Builders<UserQueryDocument>.Filter.And(
                        activeFilter,
                        Builders<UserQueryDocument>.Filter.Gte(u => u.CreatedAt, today)
                    ), cancellationToken: cancellationToken);

                var newUsersThisWeek = await _queryContext.Users.CountDocumentsAsync(
                    Builders<UserQueryDocument>.Filter.And(
                        activeFilter,
                        Builders<UserQueryDocument>.Filter.Gte(u => u.CreatedAt, weekAgo)
                    ), cancellationToken: cancellationToken);

                var newUsersThisMonth = await _queryContext.Users.CountDocumentsAsync(
                    Builders<UserQueryDocument>.Filter.And(
                        activeFilter,
                        Builders<UserQueryDocument>.Filter.Gte(u => u.CreatedAt, monthAgo)
                    ), cancellationToken: cancellationToken);

                // Calculate averages using aggregation
                var aggregationResult = await _queryContext.Users
                    .Aggregate()
                    .Match(activeFilter)
                    .Group(BsonDocument.Parse(@"{
                        _id: null,
                        avgAge: { $avg: '$age' },
                        avgProfileCompleteness: { $avg: '$profileCompleteness' }
                    }"))
                    .FirstOrDefaultAsync(cancellationToken);

                var avgAge = aggregationResult?.GetValue("avgAge", 0.0).ToDouble() ?? 0.0;
                var avgProfileCompleteness = aggregationResult?.GetValue("avgProfileCompleteness", 0.0).ToDouble() ?? 0.0;

                return new UserStatsDto
                {
                    TotalUsers = (int)totalUsers,
                    ActiveUsers = (int)totalUsers, // All non-deleted users are active
                    NewUsersToday = (int)newUsersToday,
                    NewUsersThisWeek = (int)newUsersThisWeek,
                    NewUsersThisMonth = (int)newUsersThisMonth,
                    AverageAge = avgAge,
                    AverageProfileCompleteness = avgProfileCompleteness
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                throw;
            }
        }
    }
}
