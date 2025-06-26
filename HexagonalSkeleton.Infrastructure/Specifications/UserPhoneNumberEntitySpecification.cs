using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for filtering users by phone number at entity level
    /// Provides efficient database-level filtering for exact phone number match
    /// </summary>
    public class UserPhoneNumberEntitySpecification : Specification<UserEntity>
    {
        private readonly string _phoneNumber;

        public UserPhoneNumberEntitySpecification(string phoneNumber)
        {
            _phoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        }

        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            return entity => entity.PhoneNumber != null && entity.PhoneNumber == _phoneNumber;
        }
    }
}
