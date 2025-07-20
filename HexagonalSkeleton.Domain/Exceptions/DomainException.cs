namespace HexagonalSkeleton.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all domain-related business rule violations.
    /// Follows DDD principles by encapsulating domain-specific errors.
    /// All domain exceptions should inherit from this class to maintain consistency.
    /// </summary>
    public abstract class DomainException : Exception
    {
        /// <summary>
        /// The domain context where the exception occurred (e.g., "User", "Order")
        /// </summary>
        public string DomainContext { get; }

        /// <summary>
        /// Specific business rule that was violated
        /// </summary>
        public string BusinessRule { get; }

        protected DomainException(string domainContext, string businessRule, string message) 
            : base(message) 
        { 
            DomainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
            BusinessRule = businessRule ?? throw new ArgumentNullException(nameof(businessRule));
        }

        protected DomainException(string domainContext, string businessRule, string message, Exception innerException) 
            : base(message, innerException) 
        {
            DomainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
            BusinessRule = businessRule ?? throw new ArgumentNullException(nameof(businessRule));
        }

        /// <summary>
        /// Provides a structured error message with domain context
        /// </summary>
        public override string ToString()
        {
            return $"[{DomainContext}] Business Rule Violation: {BusinessRule} - {Message}";
        }
    }
}
