using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering users by age range
    /// Useful for filtering adult users or specific age groups
    /// </summary>
    public class UserAgeRangeSpecification : Specification<User>
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public UserAgeRangeSpecification(int minAge, int maxAge)
        {
            if (minAge < 0)
                throw new ArgumentException("Minimum age cannot be negative", nameof(minAge));
            
            if (maxAge < minAge)
                throw new ArgumentException("Maximum age cannot be less than minimum age", nameof(maxAge));

            _minAge = minAge;
            _maxAge = maxAge;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            var today = DateTime.Today;
            var maxBirthDate = today.AddYears(-_minAge);
            var minBirthDate = today.AddYears(-_maxAge - 1);

            return user => user.Birthdate.HasValue && 
                          user.Birthdate.Value <= maxBirthDate && 
                          user.Birthdate.Value > minBirthDate;
        }
    }
}
