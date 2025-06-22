using HexagonalSkeleton.Domain.Specifications;
using System.Linq.Expressions;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for searching users by text
    /// Encapsulates the business logic for user search
    /// </summary>
    public class UserSearchSpecification : Specification<User>
    {
        private readonly string _searchTerm;

        public UserSearchSpecification(string searchTerm)
        {
            _searchTerm = searchTerm?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(searchTerm));
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => 
                user.FullName.FirstName.ToLowerInvariant().Contains(_searchTerm) ||
                user.FullName.LastName.ToLowerInvariant().Contains(_searchTerm) ||
                user.Email.Value.ToLowerInvariant().Contains(_searchTerm);
        }
    }

    /// <summary>
    /// Specification for finding user by email
    /// </summary>
    public class UserByEmailSpecification : Specification<User>
    {
        private readonly string _email;

        public UserByEmailSpecification(string email)
        {
            _email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => user.Email.Value.ToLowerInvariant() == _email;
        }
    }

    /// <summary>
    /// Specification for finding user by phone number
    /// </summary>
    public class UserByPhoneNumberSpecification : Specification<User>
    {
        private readonly string _phoneNumber;

        public UserByPhoneNumberSpecification(string phoneNumber)
        {
            _phoneNumber = phoneNumber?.Trim() ?? throw new ArgumentNullException(nameof(phoneNumber));
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => user.PhoneNumber.Value == _phoneNumber;
        }
    }

    /// <summary>
    /// Specification for finding user by ID
    /// </summary>
    public class UserByIdSpecification : Specification<User>
    {
        private readonly int _id;

        public UserByIdSpecification(int id)
        {
            if (id <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(id));
            
            _id = id;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            return user => user.Id == _id;
        }
    }
}
