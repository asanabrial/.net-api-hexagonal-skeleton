using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering users with complete profiles
    /// Business rule: complete profile includes all required fields
    /// </summary>
    public class CompleteProfileSpecification : Specification<User>
    {
        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => 
                !string.IsNullOrEmpty(user.FullName.FirstName) &&
                !string.IsNullOrEmpty(user.FullName.LastName) &&
                !string.IsNullOrEmpty(user.Email.Value) &&
                !string.IsNullOrEmpty(user.PhoneNumber.Value) &&
                user.Birthdate.HasValue &&
                !string.IsNullOrEmpty(user.AboutMe);
        }
    }
}
