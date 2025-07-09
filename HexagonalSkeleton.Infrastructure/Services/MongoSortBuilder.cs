using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for building MongoDB sort definitions
    /// Follows Single Responsibility Principle by only handling sorting logic
    /// </summary>
    public interface IMongoSortBuilder
    {
        SortDefinition<UserQueryDocument> GetSortDefinition(string? sortBy, bool isDescending);
    }

    public class MongoSortBuilder : IMongoSortBuilder
    {
        /// <summary>
        /// Get the appropriate sort definition based on the field name and direction
        /// Centralizes sorting logic for reusability and maintenance
        /// </summary>
        public SortDefinition<UserQueryDocument> GetSortDefinition(string? sortBy, bool isDescending)
        {
            var builder = Builders<UserQueryDocument>.Sort;
            
            if (string.IsNullOrWhiteSpace(sortBy))
                return isDescending ? builder.Descending(u => u.CreatedAt) : builder.Ascending(u => u.CreatedAt);
                
            return sortBy.ToLowerInvariant() switch
            {
                "firstname" => isDescending ? builder.Descending(u => u.FullName.FirstName) : builder.Ascending(u => u.FullName.FirstName),
                "lastname" => isDescending ? builder.Descending(u => u.FullName.LastName) : builder.Ascending(u => u.FullName.LastName),
                "email" => isDescending ? builder.Descending(u => u.Email) : builder.Ascending(u => u.Email),
                "createdat" => isDescending ? builder.Descending(u => u.CreatedAt) : builder.Ascending(u => u.CreatedAt),
                "lastlogin" => isDescending ? builder.Descending(u => u.LastLogin) : builder.Ascending(u => u.LastLogin),
                _ => isDescending ? builder.Descending(u => u.CreatedAt) : builder.Ascending(u => u.CreatedAt)
            };
        }
    }
}
