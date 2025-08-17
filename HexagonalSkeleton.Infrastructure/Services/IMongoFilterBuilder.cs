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
}
