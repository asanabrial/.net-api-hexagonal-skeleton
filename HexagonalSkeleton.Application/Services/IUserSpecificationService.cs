using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.Application.Services
{
    /// <summary>
    /// Service responsible for building user specifications from query parameters
    /// Follows Single Responsibility Principle - only handles specification building
    /// Abstracts the complexity of specification construction from the handler
    /// </summary>
    public interface IUserSpecificationService
    {
        /// <summary>
        /// Builds a specification for filtering users based on query parameters
        /// </summary>
        /// <param name="query">Query parameters containing filter criteria</param>
        /// <returns>Specification for filtering users</returns>
        ISpecification<User> BuildSpecification(GetAllUsersQuery query);
    }
}
