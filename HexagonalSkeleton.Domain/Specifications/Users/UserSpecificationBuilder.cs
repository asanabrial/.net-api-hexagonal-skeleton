using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications.Users;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Builder pattern for composing User specifications
    /// Provides fluent API for building complex filter criteria
    /// Follows SOLID principles and makes code more readable
    /// </summary>
    public class UserSpecificationBuilder
    {
        private Specification<User>? _specification;

        /// <summary>
        /// Start with active users only (business rule: exclude deleted users)
        /// </summary>
        public UserSpecificationBuilder OnlyActive()
        {
            AddSpecification(new ActiveUserSpecification());
            return this;
        }

        /// <summary>
        /// Filter by search term across multiple fields
        /// </summary>
        public UserSpecificationBuilder WithSearchTerm(string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                AddSpecification(new UserTextSearchSpecification(searchTerm));
            }
            return this;
        }
        /// <summary>
        /// Filter by age range
        /// </summary>
        public UserSpecificationBuilder WithAgeRange(int minAge, int maxAge)
        {
            AddSpecification(new UserAgeRangeSpecification(minAge, maxAge));
            return this;
        }

        /// <summary>
        /// Filter only adult users (18+)
        /// </summary>
        public UserSpecificationBuilder OnlyAdults()
        {
            AddSpecification(new AdultUserSpecification());
            return this;
        }

        /// <summary>
        /// Filter by location within radius
        /// </summary>
        public UserSpecificationBuilder WithinLocation(double latitude, double longitude, double radiusInKm)
        {
            AddSpecification(new UserLocationSpecification(latitude, longitude, radiusInKm));
            return this;
        }

        /// <summary>
        /// Filter users with complete profiles only
        /// </summary>
        public UserSpecificationBuilder WithCompleteProfile()
        {
            AddSpecification(new CompleteProfileSpecification());
            return this;
        }

        /// <summary>
        /// Add custom specification
        /// </summary>
        public UserSpecificationBuilder WithCustomSpecification(Specification<User> specification)
        {
            if (specification != null)
            {
                AddSpecification(specification);
            }
            return this;
        }

        /// <summary>
        /// Build the final specification
        /// Returns a specification that matches all users if no criteria were added
        /// </summary>
        public ISpecification<User> Build()
        {
            return _specification ?? new AllUsersSpecification();
        }

        private void AddSpecification(Specification<User> specification)
        {
            _specification = _specification == null 
                ? specification 
                : _specification.And(specification);
        }

        /// <summary>
        /// Factory method for creating a new builder
        /// </summary>
        public static UserSpecificationBuilder Create()
        {
            return new UserSpecificationBuilder();
        }
    }
}
