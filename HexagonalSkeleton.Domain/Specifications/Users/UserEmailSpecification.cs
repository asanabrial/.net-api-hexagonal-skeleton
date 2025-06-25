using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering users by email
    /// Provides exact match filtering for email address
    /// </summary>
    public class UserEmailSpecification : Specification<User>
    {
        private readonly string _email;

        public UserEmailSpecification(string email)
        {
            _email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => user.Email.Value.ToLower() == _email;
        }
    }
}
