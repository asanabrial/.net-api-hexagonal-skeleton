using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for filtering users with complete profiles at entity level
    /// Provides efficient database-level filtering for profile completeness
    /// </summary>
    public class CompleteProfileEntitySpecification : Specification<UserEntity>
    {
        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            return entity => 
                !string.IsNullOrEmpty(entity.Name) &&
                !string.IsNullOrEmpty(entity.Surname) &&
                !string.IsNullOrEmpty(entity.Email) &&
                !string.IsNullOrEmpty(entity.PhoneNumber) &&
                entity.Birthdate.HasValue &&
                !string.IsNullOrEmpty(entity.AboutMe);
        }
    }
}
