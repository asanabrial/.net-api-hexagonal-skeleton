using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for filtering active users at entity level
    /// Works directly with UserEntity for optimal database performance
    /// </summary>
    public class ActiveUserEntitySpecification : Specification<UserEntity>
    {
        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            return entity => !entity.IsDeleted;
        }
    }
}
