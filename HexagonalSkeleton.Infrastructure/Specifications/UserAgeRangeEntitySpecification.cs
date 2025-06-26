using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for filtering users by age range at entity level
    /// Provides efficient database-level filtering for age-based queries
    /// </summary>
    public class UserAgeRangeEntitySpecification : Specification<UserEntity>
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public UserAgeRangeEntitySpecification(int minAge, int maxAge)
        {
            if (minAge < 0)
                throw new ArgumentException("Minimum age cannot be negative", nameof(minAge));
            
            if (maxAge < minAge)
                throw new ArgumentException("Maximum age cannot be less than minimum age", nameof(maxAge));

            _minAge = minAge;
            _maxAge = maxAge;
        }

        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            var today = DateTime.Today;
            var maxBirthDate = today.AddYears(-_minAge);
            var minBirthDate = today.AddYears(-_maxAge - 1);

            return entity => entity.Birthdate.HasValue && 
                           entity.Birthdate.Value <= maxBirthDate && 
                           entity.Birthdate.Value > minBirthDate;
        }
    }
}
