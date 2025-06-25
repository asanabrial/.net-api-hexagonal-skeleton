using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification that matches all users (no filtering)
    /// Used as a default when no specific criteria are provided
    /// </summary>
    public class AllUsersSpecification : Specification<User>
    {
        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => true; // Matches all users
        }
    }
}
