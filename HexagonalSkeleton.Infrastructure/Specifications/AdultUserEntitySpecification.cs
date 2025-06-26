using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for filtering adult users at entity level
    /// Provides efficient database-level filtering for adult users (18+)
    /// </summary>
    public class AdultUserEntitySpecification : Specification<UserEntity>
    {
        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            var eighteenYearsAgo = DateTime.Today.AddYears(-18);
            
            return entity => entity.Birthdate.HasValue && entity.Birthdate.Value <= eighteenYearsAgo;
        }
    }
}
