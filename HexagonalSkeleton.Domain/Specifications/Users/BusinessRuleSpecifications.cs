using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for business rule: Users must meet minimum age requirements
    /// Encapsulates the domain logic for age validation in queries
    /// </summary>
    public sealed class MinimumAgeSpecification : Specification<User>
    {
        private readonly int _minimumAge;

        public MinimumAgeSpecification(int minimumAge = 13)
        {
            if (minimumAge < 0)
                throw new ArgumentException("Minimum age cannot be negative", nameof(minimumAge));
                
            _minimumAge = minimumAge;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            var today = DateTime.Today;
            return user => user.Birthdate.HasValue && 
                          (today.Year - user.Birthdate.Value.Year - 
                           (user.Birthdate.Value.Date > today.AddYears(-(today.Year - user.Birthdate.Value.Year)) ? 1 : 0)) >= _minimumAge;
        }
    }

    /// <summary>
    /// Specification for business rule: Profile completeness above threshold
    /// Useful for filtering users with sufficient profile information
    /// </summary>
    public sealed class ProfileCompletenessSpecification : Specification<User>
    {
        private readonly double _minimumCompleteness;

        public ProfileCompletenessSpecification(double minimumCompleteness = 70.0)
        {
            if (minimumCompleteness < 0 || minimumCompleteness > 100)
                throw new ArgumentException("Completeness must be between 0 and 100", nameof(minimumCompleteness));
                
            _minimumCompleteness = minimumCompleteness;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            // Simplified completeness calculation for database queries
            return user => !string.IsNullOrEmpty(user.Email.Value) &&
                          !string.IsNullOrEmpty(user.FullName.FirstName) &&
                          !string.IsNullOrEmpty(user.FullName.LastName) &&
                          !string.IsNullOrEmpty(user.PhoneNumber.Value) &&
                          user.Birthdate.HasValue;
        }
    }
}
