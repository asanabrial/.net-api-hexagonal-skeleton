using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering users by phone number
    /// Provides exact match filtering for phone number
    /// </summary>
    public class UserPhoneNumberSpecification : Specification<User>
    {
        private readonly string _phoneNumber;

        public UserPhoneNumberSpecification(string phoneNumber)
        {
            _phoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => user.PhoneNumber.Value == _phoneNumber;
        }
    }
}
