using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for general text search across user fields
    /// Performs partial matching (contains) on: first name, last name, email, and phone number
    /// This is different from exact match specifications which target specific fields
    /// Uses case-insensitive search for better user experience
    /// Example: searching "john" will find users with "John" in first name, "johnson@email.com", etc.
    /// </summary>
    public class UserTextSearchSpecification : Specification<User>
    {
        private readonly string _searchTerm;

        public UserTextSearchSpecification(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
            
            _searchTerm = searchTerm.ToLowerInvariant().Trim();
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => 
                user.FullName.FirstName.ToLower().Contains(_searchTerm) ||
                user.FullName.LastName.ToLower().Contains(_searchTerm) ||
                user.Email.Value.ToLower().Contains(_searchTerm) ||
                user.PhoneNumber.Value.Contains(_searchTerm);
        }
    }
}
