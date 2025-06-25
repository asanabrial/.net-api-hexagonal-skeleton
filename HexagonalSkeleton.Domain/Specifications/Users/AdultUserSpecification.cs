using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering adult users (18+ years old)
    /// Common business rule that can be composed with other specifications
    /// </summary>
    public class AdultUserSpecification : Specification<User>
    {
        public override Expression<Func<User, bool>> ToExpression()
        {
            var eighteenYearsAgo = DateTime.Today.AddYears(-18);
            
            return user => user.Birthdate.HasValue && user.Birthdate.Value <= eighteenYearsAgo;
        }
    }
}
