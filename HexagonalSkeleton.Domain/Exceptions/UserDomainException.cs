namespace HexagonalSkeleton.Domain.Exceptions
{
    /// <summary>
    /// Specific domain exception for User aggregate business rule violations.
    /// Provides structured error information for user-related domain errors.
    /// </summary>
    public sealed class UserDomainException : DomainException
    {
        public UserDomainException(string businessRule, string message) 
            : base("User", businessRule, message)
        {
        }

        public UserDomainException(string businessRule, string message, Exception innerException) 
            : base("User", businessRule, message, innerException)
        {
        }

        /// <summary>
        /// Factory method for age validation errors
        /// </summary>
        public static UserDomainException InvalidAge(int age, int minimumAge) =>
            new("MinimumAgeRequirement", $"User age {age} is below the minimum required age of {minimumAge}");

        /// <summary>
        /// Factory method for deleted user operation errors
        /// </summary>
        public static UserDomainException OperationOnDeletedUser(string operation) =>
            new("DeletedUserOperation", $"Cannot perform '{operation}' on a deleted user");

        /// <summary>
        /// Factory method for email validation errors
        /// </summary>
        public static UserDomainException InvalidEmail(string email) =>
            new("EmailValidation", $"The email '{email}' is not in a valid format");

        /// <summary>
        /// Factory method for phone number validation errors
        /// </summary>
        public static UserDomainException InvalidPhoneNumber(string phoneNumber) =>
            new("PhoneNumberValidation", $"The phone number '{phoneNumber}' is not in a valid format");
    }
}
