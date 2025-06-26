using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;
using HexagonalSkeleton.Infrastructure.Persistence.Entities;

namespace HexagonalSkeleton.Infrastructure.Specifications
{
    /// <summary>
    /// Infrastructure specification for searching users by text at entity level
    /// Provides efficient database-level filtering for text search
    /// </summary>
    public class UserTextSearchEntitySpecification : Specification<UserEntity>
    {
        private readonly string _searchTerm;

        public UserTextSearchEntitySpecification(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
            
            _searchTerm = searchTerm.ToLowerInvariant().Trim();
        }

        public override Expression<Func<UserEntity, bool>> ToExpression()
        {
            return entity => 
                (entity.Name != null && entity.Name.ToLower().Contains(_searchTerm)) ||
                (entity.Surname != null && entity.Surname.ToLower().Contains(_searchTerm)) ||
                (entity.Email != null && entity.Email.ToLower().Contains(_searchTerm)) ||
                (entity.PhoneNumber != null && entity.PhoneNumber.Contains(_searchTerm));
        }
    }
}
