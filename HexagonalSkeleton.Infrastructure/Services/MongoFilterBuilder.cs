using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for building MongoDB filters from domain specifications
    /// Follows Single Responsibility Principle by only handling filter creation
    /// </summary>
    public interface IMongoFilterBuilder
    {
        FilterDefinition<UserQueryDocument> ConvertSpecificationToMongoFilter(ISpecification<User> specification);
    }

    public class MongoFilterBuilder : IMongoFilterBuilder
    {
        /// <summary>
        /// Converts domain specification to MongoDB filter
        /// This implementation can be extended to handle complex specifications
        /// </summary>
        public FilterDefinition<UserQueryDocument> ConvertSpecificationToMongoFilter(ISpecification<User> specification)
        {
            // Create a default filter (always true)
            var filter = Builders<UserQueryDocument>.Filter.Empty;
            
            // In a real implementation, you'd parse the specification's criteria
            // and convert them to MongoDB filters
            // This could use reflection or a specification visitor pattern
            
            return filter;
        }
    }
}
