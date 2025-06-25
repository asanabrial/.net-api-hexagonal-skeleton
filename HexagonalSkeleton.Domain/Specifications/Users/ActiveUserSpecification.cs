using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering active users (not deleted)
    /// This is a common business rule that should be enforced across queries
    /// </summary>
    public class ActiveUserSpecification : Specification<User>
    {
        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => !user.IsDeleted;
        }
    }
}
