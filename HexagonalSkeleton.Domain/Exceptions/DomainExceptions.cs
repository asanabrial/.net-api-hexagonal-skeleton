namespace HexagonalSkeleton.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all domain-related business rule violations
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when attempting to create a user with data that already exists
    /// </summary>
    public class UserDataNotUniqueException : DomainException
    {
        public string Email { get; }
        public string? PhoneNumber { get; }

        public UserDataNotUniqueException(string email, string? phoneNumber = null) 
            : base(BuildMessage(email, phoneNumber))
        {
            Email = email;
            PhoneNumber = phoneNumber;
        }

        private static string BuildMessage(string email, string? phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
                return $"User with email '{email}' or phone number '{phoneNumber}' already exists";
            return $"User with email '{email}' already exists";
        }
    }

    /// <summary>
    /// Exception thrown when password doesn't meet business requirements
    /// </summary>
    public class WeakPasswordException : DomainException
    {
        public WeakPasswordException() 
            : base("Password does not meet strength requirements")
        {
        }
    }
}
