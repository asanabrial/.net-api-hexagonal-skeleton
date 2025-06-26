using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for filtering users by email at entity level
    /// Provides efficient database-level filtering for exact email match
    /// </summary>
    public class UserEmailEntitySpecification : Specification<UserEntity>
    {
        private readonly string _email;

        public UserEmailEntitySpecification(string email)
        {
            _email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        }

        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            return entity => entity.Email != null && entity.Email.ToLower() == _email;
        }
    }
}
