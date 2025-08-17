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
}
